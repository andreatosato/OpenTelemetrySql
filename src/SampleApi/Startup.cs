using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
            services.AddOpenTelemetryTracing((tracerBuilder) =>
            {
                tracerBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddZipkinExporter();

                tracerBuilder
                   .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Configuration.GetValue<string>("NewRelic:ServiceName")))
                   .AddNewRelicExporter(options =>
                   {
                       options.ApiKey = this.Configuration.GetValue<string>("NewRelic:ApiKey");
                   })
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation();
            });

            services.Configure<ZipkinExporterOptions>(this.Configuration.GetSection("Zipkin"));
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
