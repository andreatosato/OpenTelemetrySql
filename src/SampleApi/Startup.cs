using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleApi.Messages;
using SampleDatabase;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleApi", Version = "v1" });
            });
            services.AddSingleton<MessageSender>();

            services.AddDbContext<SampleContext>(x =>
            {
                x.UseSqlServer(Configuration.GetConnectionString("Default"))
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddOpenTelemetryTracing((builder) => builder
                        .AddSource("Sample")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SampleApi").AddTelemetrySdk())
                        .AddSqlClientInstrumentation(s =>
                        {
                            s.SetDbStatementForStoredProcedure = true;
                            s.SetDbStatementForText = true;
                            s.RecordException = true;
                        })
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = (req) => !req.Request.Path.ToUriComponent().Contains("swagger", StringComparison.OrdinalIgnoreCase);
                        })
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter()
                        //.AddNewRelicExporter(options =>
                        //{
                        //    options.ApiKey = Configuration.GetValue<string>("NewRelic:ApiKey");
                        //    options.Endpoint = new Uri("https://metric-api.eu.newrelic.com/trace/v1");
                        //})
                        .AddZipkinExporter(b =>
                        {
                            var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                            b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
                        })
                        .AddJaegerExporter(b =>
                        {
                            b.AgentHost = "jaeger";
                            b.AgentPort = 6831;
                        })
                    );
            services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
            services.Configure<JaegerExporterOptions>(this.Configuration.GetSection("Jaeger"));

            services.AddHttpClient("SampleApiTre", h =>
            {
                h.BaseAddress = new Uri("http://sampleapitre/");
            });

            services.AddHttpClient("SampleApiDue", h =>
            {
                h.BaseAddress = new Uri("https://sampleapidue/");
            })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { ServerCertificateCustomValidationCallback = (HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors) => true });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleContext db)
        {
            if (env.IsDevelopment())
            {
                db.Database.Migrate();

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
