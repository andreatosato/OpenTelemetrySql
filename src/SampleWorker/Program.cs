using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SampleWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateDatabase();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    var tracerProvider = Sdk.CreateTracerProviderBuilder()
                        .SetSampler(new AlwaysOnSampler())
                        .AddSource("SampleWorker", "Reader", "Database")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SampleWorker").AddEnvironmentVariableDetector().AddTelemetrySdk())
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation(s =>
                        {
                            s.EnableConnectionLevelAttributes = true;
                            s.RecordException = true;
                            s.SetDbStatementForText = true;
                            s.SetDbStatementForStoredProcedure = false;
                        })
                        .AddZipkinExporter(b =>
                        {
                            var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                            b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
                        })
                        .AddJaegerExporter(j =>
                        {
                            j.AgentHost = "jaeger";
                            j.AgentPort = 6831;
                        })
                      .Build();
                    services.Configure<AspNetCoreInstrumentationOptions>(hostContext.Configuration.GetSection("AspNetCoreInstrumentation"));
                });

        public static async Task CreateDatabase()
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("SqlConnection"))
            {
                InitialCatalog = "master"
            };
            using var connectionMaster = new SqlConnection(sqlBuilder.ToString());
            connectionMaster.Open();
            await new SqlCommand(@"IF EXISTS (SELECT name FROM master.sys.databases WHERE name = N'Worker')
                DROP DATABASE [Worker]", connectionMaster).ExecuteNonQueryAsync();
            await new SqlCommand("CREATE DATABASE [Worker]", connectionMaster).ExecuteNonQueryAsync();
            await connectionMaster.CloseAsync();

            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnection"));
            connection.Open();
            await new SqlCommand(@"CREATE TABLE [dbo].[User](
                [Id][int] IDENTITY(1, 1) NOT NULL,
                [Name] [nvarchar](max)NULL,
	            [Surname] [nvarchar](max)NULL,
             CONSTRAINT[PK_User] PRIMARY KEY CLUSTERED([Id] ASC)
            )", connection).ExecuteNonQueryAsync();



            await new SqlCommand(@"CREATE TABLE [dbo].[Vote](
	            [Id][int] IDENTITY(1, 1) NOT NULL,
	            [IdUser] [int] NOT NULL,
	            [VoteValue] [int] NOT NULL,
	            [Date] [datetime] NOT NULL,
             CONSTRAINT [PK_Vote] PRIMARY KEY CLUSTERED ([Id] ASC)
            )", connection).ExecuteNonQueryAsync();


            await new SqlCommand(@"ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_User] FOREIGN KEY([Id])
            REFERENCES [dbo].[User] ([Id])", connection).ExecuteNonQueryAsync();


            await new SqlCommand(@"ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_User]", connection).ExecuteNonQueryAsync();

            await new SqlCommand(@"ALTER TABLE [dbo].[Vote]  WITH CHECK ADD  CONSTRAINT [FK_Vote_User] FOREIGN KEY([IdUser])
            REFERENCES [dbo].[User] ([Id])
            ON DELETE CASCADE", connection).ExecuteNonQueryAsync();

            await new SqlCommand(@"ALTER TABLE [dbo].[Vote] CHECK CONSTRAINT [FK_Vote_User]", connection).ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }
    }
}
