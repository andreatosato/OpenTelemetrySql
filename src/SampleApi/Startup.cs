using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;
using SampleApi.Messages;

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

            services.AddOpenTelemetryTracing((builder) => builder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddNewRelicExporter(options => options.ApiKey = Configuration.GetValue<string>("NewRelic:ApiKey"))
                        .AddZipkinExporter(b =>
                        {
                            var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                            b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
                        }));
            services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
