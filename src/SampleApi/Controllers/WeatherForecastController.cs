﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleApi.Messages;
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
        private readonly MessageSender messageSender;
        private readonly ActivitySource s_source = new ActivitySource("Sample");
        private readonly SampleContext db;
        private readonly HttpClient sampleApiDueClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            MessageSender messageSender,
            SampleContext db,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.sampleApiDueClient = httpClientFactory.CreateClient("SampleApiDue");
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            using (Activity activity = s_source.StartActivity("StartController"))
            {
                using (Activity activityMessage = s_source.StartActivity("Rabbit").SetParentId(activity.Id).SetTag("type", "RabbitType"))
                {
                    _logger.LogInformation("Send message data to rabbitmq");
                    activityMessage.AddBaggage("baggage-name", "my-value-baggage");
                    messageSender.SendMessage();
                    activityMessage.Stop();
                }

                using (Activity activityMessage = s_source.StartActivity("Google Activity").SetParentId(activity.Id).SetTag("type", "GoogleType"))
                {
                    _logger.LogInformation("SampleApi running at: {time}", DateTimeOffset.Now);
                    var res = await new HttpClient().GetStringAsync("http://google.com");
                    activityMessage.Stop();
                }

                using (Activity activityMessage = s_source.StartActivity("Sample API 2 Activity").SetParentId(activity.Id).SetTag("type", "API 2 Type"))
                {
                    _logger.LogInformation("SampleApi running at: {time}", DateTimeOffset.Now);
                    var res = await sampleApiDueClient.GetStringAsync("api/WeatherForecast");
                    _logger.LogInformation("Data from API 2: {@res}", res);
                    activityMessage.Stop();
                }

                using (Activity activityMessage = s_source.StartActivity("Database").SetParentId(activity.Id).SetTag("type", "DatanaseType"))
                {
                    _logger.LogInformation("SampleApi running at: {time}", DateTimeOffset.Now);
                    var blog = await db.Blogs
                                        .Include(x => x.PostUsers)
                                            .ThenInclude(t => t.User)
                                        .Include(x => x.PostUsers)
                                            .ThenInclude(t => t.Post)
                                        .ToListAsync();
                    activityMessage.Stop();
                }

                using (Activity activityMessage = s_source.StartActivity("Random").SetParentId(activity.Id).SetTag("type", "RandomType"))
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

                activity.Stop();
            }
        }
    }
}
