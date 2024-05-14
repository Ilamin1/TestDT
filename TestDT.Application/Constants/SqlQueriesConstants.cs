namespace TestDT.Application.Constants;

public class SqlQueriesConstants
{
    public const string CreateMainTableQuery = $$"""
                                               
                                                           CREATE TABLE {{DbConstants.TableName}}
                                                           (
                                                               tpep_pickup_datetime DATETIME,
                                                               tpep_dropoff_datetime DATETIME,
                                                               passenger_count INT,
                                                               trip_distance DECIMAL(10, 2),
                                                               store_and_fwd_flag VARCHAR(3),
                                                               PULocationID INT,
                                                               DOLocationID INT,
                                                               fare_amount DECIMAL(10, 2),
                                                               tip_amount DECIMAL(10, 2)
                                                           );
                                               
                                                           CREATE CLUSTERED INDEX IX_PULocationID ON {{DbConstants.TableName}} (PULocationID);
                                                           CREATE INDEX IX_tip_amount ON {{DbConstants.TableName}} (tip_amount);
                                                           CREATE INDEX IX_trip_distance ON {{DbConstants.TableName}} (trip_distance);
                                                           CREATE INDEX IX_pickup_datetime ON {{DbConstants.TableName}} (tpep_pickup_datetime);
                                                           CREATE INDEX IX_dropoff_datetime ON {{DbConstants.TableName}} (tpep_dropoff_datetime);
                                               """;

    public const string CheckDbExist =
        "SELECT COUNT(*) FROM master.sys.tables WHERE name = '{0}' AND schema_id = SCHEMA_ID('dbo')";

    public const string TruncateTable = "TRUNCATE TABLE {0}";
}