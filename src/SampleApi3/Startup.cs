using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SampleApi3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleApi3", Version = "v1" });
            });

            services.AddLogging();
            services.AddOpenTelemetryTracing((builder) => builder
                .AddSource("SampleTre")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SampleApiTreService").AddTelemetrySdk())
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri("http://otel-collector:4317");
                    // this.Configuration.GetValue<string>("Otlp:Endpoint"));
                })
            //.AddZipkinExporter(b =>
            //{
            //    var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
            //    b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
            //})
            );

            services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi3 v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
