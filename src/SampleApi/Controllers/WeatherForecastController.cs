using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleDatabase;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly ActivitySource source = new ActivitySource("Sample");
        private readonly SampleContext db;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            SampleContext db)
        {
            _logger = logger;
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            using (Activity activity = source.StartActivity("StartController"))
            {
                _logger.LogInformation("Randomize data");
                var rng = new Random();
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
            }
        }
    }
}
