using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using TestDT.Application.Constants;
using TestDT.Application.Models.Mediators.Commands;

namespace TestDT.Api.Extensions;

public static class ServiceCollectionDiExtensions
{
    public static void ServiceCollectionDi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DataUploadCommand).Assembly));
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" }); });
        services.AddCors(options =>
        {
            options.AddPolicy("TestDTSettings", _ =>
            {
            });
        });
    }

    public static void SetupDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("DataBaseConnectionString").Value;
        var sqlConnection = new SqlConnection(connectionString);
        services.AddSingleton<IDbConnection>(_ => sqlConnection);
        CreateDbIfNotExist(sqlConnection);
    }

    private static void CreateDbIfNotExist(SqlConnection sqlConnection)
    {
        sqlConnection.Open();
        var checkDatabaseQuery = string.Format(SqlQueriesConstants.CheckDbExist, DbConstants.TableName);
        using var checkCommand = new SqlCommand(checkDatabaseQuery, sqlConnection);
        var databaseCount = (int)checkCommand.ExecuteScalar();
        if (databaseCount == 0)
        {
            using var createCommand = sqlConnection.CreateCommand();
            createCommand.CommandText = SqlQueriesConstants.CreateMainTableQuery;
            createCommand.ExecuteNonQuery();
        }
        sqlConnection.Close();
    }

    public static void SetupApplicationBuilder(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1"); });

        app.UseCors("TradingViewPolicy");

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}