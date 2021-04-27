using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleApi.Messages;

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
        private readonly MessageSender messageSender;
        private readonly ActivitySource s_source = new ActivitySource("Sample");

        public WeatherForecastController(ILogger<WeatherForecastController> logger, MessageSender messageSender)
        {
            if (messageSender == null)
            {
                throw new ArgumentNullException(nameof(messageSender));
            }

            _logger = logger;
            this.messageSender = messageSender;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            using (Activity? activity = s_source.StartActivity("StartController"))
            {
                using (Activity activityMessage = s_source.StartActivity("SendMessage"))
                {
                    _logger.LogInformation("Send message data to rabbitmq");
                    messageSender.SendMessage();
                }

                using (Activity activityMessage = s_source.StartActivity("Google"))
                {
                    _logger.LogInformation("SampleApi running at: {time}", DateTimeOffset.Now);
                    var res = await new HttpClient().GetStringAsync("http://google.com");
                }

                using (Activity activityMessage = s_source.StartActivity("Randomize"))
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
}
