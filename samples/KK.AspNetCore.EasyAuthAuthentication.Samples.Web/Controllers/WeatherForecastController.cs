namespace KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger
        ) => this.Logger = logger;

        public ILogger<WeatherForecastController> Logger { get; }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
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
