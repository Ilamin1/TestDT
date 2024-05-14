using System.Data;
using System.Globalization;
using CsvHelper;
using MediatR;
using Microsoft.Data.SqlClient;
using TestDT.Application.Constants;
using TestDT.Application.Models.Mediators.Commands;

namespace TestDT.Application.Handlers.DataUploaderHandlers;

public class DataUploaderHandler : IRequestHandler<DataUploadCommand, Unit>
{
    private readonly SqlConnection _dbConnection;
    
    public DataUploaderHandler(IDbConnection dbConnection)
    {
        this._dbConnection = (SqlConnection)dbConnection;
    }

    public async Task<Unit> Handle(DataUploadCommand request, CancellationToken cancellationToken)
    {
        await _dbConnection.OpenAsync(cancellationToken);
        TruncateTable(_dbConnection);
        await using var transaction = _dbConnection.BeginTransaction();
        try
        {
            using (var sqlBulkCopy = new SqlBulkCopy(_dbConnection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, transaction))
            {
                sqlBulkCopy.DestinationTableName = DbConstants.TableName;
                var dataTable = await GetCorrectDataTable(request.FileStream);

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            }

            transaction.Commit();
            await _dbConnection.CloseAsync();
            return Unit.Value;
        }
        catch
        {
            transaction.Rollback();
            await _dbConnection.CloseAsync();
            throw;
        }
    }

    private static void TruncateTable(SqlConnection sqlConnection)
    {
        var checkDatabaseQuery = string.Format(SqlQueriesConstants.TruncateTable, DbConstants.TableName);
        using var checkCommand = new SqlCommand(checkDatabaseQuery, sqlConnection);
        checkCommand.ExecuteNonQuery();
    }

    private static async Task<DataTable> GetCorrectDataTable(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        using var csvDataReader = new CsvDataReader(csvReader);
        var dataTable = new DataTable();
        dataTable.Load(csvDataReader);
        SetWritePermission(dataTable);
        RemoveUnnecessaryColumns(dataTable);
        RemoveEmptyRows(dataTable);
        await HandleDuplicateRows(dataTable);
        ReorderTableRows(dataTable);
        
        return dataTable;
    }
    
    private static async Task HandleDuplicateRows(DataTable dataTable)
    {
        var duplicateRows = GetDuplicates(dataTable);
        var columnNames = GetColumnNames(dataTable);
        await SaveDuplicateRowsToCsv(columnNames, duplicateRows, DataUploadConstants.DuplicatesFileName);
        RemoveDuplicateRows(dataTable, duplicateRows);
    }

    private static IEnumerable<string> GetColumnNames(DataTable dataTable)
    {
        foreach (DataColumn dataColumn in dataTable.Columns)
        {
            yield return dataColumn.ColumnName;
        }
    }

    private static void RemoveUnnecessaryColumns(DataTable dataTable)
    {
        var unnecessaryColumns = new List<DataColumn>();
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!DataUploadConstants.AllowedColumns.Any(allowedColumn => allowedColumn.Equals(column.ColumnName)))
            {
                unnecessaryColumns.Add(column);
            }
        }

        foreach (var column in unnecessaryColumns)
        {
            dataTable.Columns.Remove(column);
        }
    }

    private static void RemoveEmptyRows(DataTable dataTable)
    {
        var incorrectRows = new List<DataRow>();
        foreach (DataRow row in dataTable.Rows)
        {
            if (row.ItemArray.Any(item => string.IsNullOrEmpty(item?.ToString())))
            {
                incorrectRows.Add(row);
            }
        }

        foreach (var incorrectRow in incorrectRows)
        {
            dataTable.Rows.Remove(incorrectRow);
        }
    }

    private static IEnumerable<DataRow> GetDuplicates(DataTable dataTable)
    {
        var duplicateRows = new List<DataRow>();
        var uniqueRecords = new HashSet<string>();
        foreach (DataRow row in dataTable.Rows)
        {
            var recordKey = $"{row["tpep_pickup_datetime"]}|{row["tpep_dropoff_datetime"]}|{row["passenger_count"]}";
            // NOTE: Check if the current record is unique.
            // If the record is unique, the Add method will return true, and we add its key to the uniqueRecords HashSet.
            // If the record is not unique, the Add method will return false, and we keep it as a duplicate.
            var isRecordUnique = uniqueRecords.Add(recordKey);
            if (!isRecordUnique)
            {
                duplicateRows.Add(row);
            }
        }
        
        return duplicateRows;
    }

    private static void ReorderTableRows(DataTable dataTable)
    {
        foreach (DataRow row in dataTable.Rows)
        {
            TrimRow(row);
            ConvertFieldFlag(row);
            ConvertDateTimeFieldsToUtc(row);
        }
    }

    private static void SetWritePermission(DataTable dataTable)
    {
        foreach (DataColumn column in dataTable.Columns)
        {
            column.ReadOnly = false;
        }
    }

    private static void ConvertDateTimeFieldsToUtc(DataRow row)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            if (DateTime.TryParseExact(row[column].ToString(), "MM/dd/yyyy hh:mm:ss tt",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                var utcDateTime = dateTime.ToUniversalTime();
                row[column] = utcDateTime;
            }
        }
    }

    private static void RemoveDuplicateRows(DataTable dataTable, IEnumerable<DataRow> incorrectRows)
    {
        foreach (var incorrectRow in incorrectRows)
        {
            dataTable.Rows.Remove(incorrectRow);
        }
    }
    
    private static async Task SaveDuplicateRowsToCsv(IEnumerable<string> columnNames, IEnumerable<DataRow> incorrectRows, string filePath)
    {
        await using var streamWriter = new StreamWriter(filePath);

        await streamWriter.WriteLineAsync(string.Join(",", columnNames));

        foreach (var incorrectRow in incorrectRows)
        {
            var rowValues = incorrectRow.ItemArray;
            await streamWriter.WriteLineAsync(string.Join(",", rowValues));
        }
    }

    private static void TrimRow(DataRow row)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            row[column] = row[column].ToString()?.Trim();
        }
    }

    private static void ConvertFieldFlag(DataRow row)
    {
        row[DataUploadConstants.FlagColumnName] = row[DataUploadConstants.FlagColumnName].ToString() switch
        {
            "N" => "No",
            "Y" => "Yes",
            _ => row[DataUploadConstants.FlagColumnName]
        };
    }
}