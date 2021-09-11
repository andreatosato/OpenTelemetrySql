using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

            services.AddDbContext<SampleContext>(x =>
            {
                x.UseSqlServer(Configuration.GetConnectionString("Default"))
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .LogTo((m) =>
                {
                    ActivitySource source = new ActivitySource("EF-Core");
                    var activityMessage = source.StartActivity("Insert-Statement");
                    activityMessage?.AddTag("db.statement", m);
                },
                new[] { RelationalEventId.CommandExecuted, RelationalEventId.CommandExecuting, RelationalEventId.ConnectionClosed }
                );
            });

            services.AddOpenTelemetryTracing((builder) => builder
                        .AddSource("Sample", "EF-Core")
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
                        //.AddAzureMonitorTraceExporter(o =>
                        //{
                        //    o.ConnectionString = "InstrumentationKey=61ac831c-6667-401f-ba62-962b20f604a1;IngestionEndpoint=https://westeurope-2.in.applicationinsights.azure.com/";
                        //})
                        //.AddZipkinExporter(b =>
                        //{
                        //    b.Endpoint = new Uri($"http://zipkin:9411/api/v2/spans");
                        //})
                        .AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri("http://otel-collector:4317");
                        })

                    );
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
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
