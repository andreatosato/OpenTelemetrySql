using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
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
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SampleWorker").AddTelemetrySdk())
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter(otlpOptions =>
                            {
                                otlpOptions.Endpoint = new Uri("http://otel-collector:4317");
                                // this.Configuration.GetValue<string>("Otlp:Endpoint"));
                            });
                        //.AddZipkinExporter(b =>
                        //{
                        //    var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                        //    b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
                        //});
                    });
                    services.Configure<AspNetCoreInstrumentationOptions>(hostContext.Configuration.GetSection("AspNetCoreInstrumentation"));
                });


    }
}
