using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventTransferObjects;
using OrderService.ApplicationServices;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.EventBus.Distributed;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : AbpController
    {

        private readonly UserAppService _userAppService;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            UserAppService userAppService
        )
        {
            _logger = logger;
            _userAppService = userAppService;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

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
        
        [HttpGet]
        [Route("user-address")]
        public async Task<string> GetUserAddress(string address)
        {
            return await _userAppService.UpdateAddress("getaddress:" + address);
        }

    }
}
