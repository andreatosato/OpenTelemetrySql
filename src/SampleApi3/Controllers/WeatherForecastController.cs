using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleApi3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
       {
            "Caldo", "Freddo", "Sereno e variabile"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ActivitySource s_source = new ActivitySource("SampleTre");

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            using var activity = s_source.StartActivity("RandomizeResponse");
            activity.AddEvent(new ActivityEvent("Generating Data"));

            var rng = new Random();
            activity.AddEvent(new ActivityEvent("Crea oggetto random"));
            var lista = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            activity.AddEvent(new ActivityEvent(JsonSerializer.Serialize(lista)));

            return lista;
        }
    }
}
