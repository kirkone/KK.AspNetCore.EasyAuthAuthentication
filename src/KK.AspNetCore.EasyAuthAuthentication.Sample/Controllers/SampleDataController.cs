namespace KK.AspNetCore.EasyAuthAuthentication.Sample.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "SystemAdmin")]
        public string UserName()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value because this is use for debugging.
            var name = this.User.HasClaim(ClaimTypes.Name, "user@somecloud.onmicrosoft.com");
            var peng = this.User.HasClaim(ClaimTypes.Role, "SystemAdmin");
            var blubb = this.HttpContext.User.IsInRole("SystemAdmin");
            var pop = this.User.IsInRole("SystemAdmin");
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            return this.HttpContext.User.Identity.Name;
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF => 32 + (int)( this.TemperatureC / 0.5556 );
        }
    }
}
