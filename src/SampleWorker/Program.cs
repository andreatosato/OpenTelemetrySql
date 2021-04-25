using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;
using SampleWorker.Messages;

namespace SampleWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();


                    services.AddSingleton<MessageReceiver>();

                    services.AddOpenTelemetryTracing((builder) =>
                    {
                        builder
                            .AddSource(nameof(MessageReceiver))
                            .AddZipkinExporter();
                    });
                    services.Configure<AspNetCoreInstrumentationOptions>(hostContext.Configuration.GetSection("AspNetCoreInstrumentation"));
                });


    }
}
