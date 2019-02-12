using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostForecastController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public CostForecastController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<LinearRegressionModel>> Get([FromBody] LinearRegressionModel data)
        {
            Forecast forecast = new Forecast();
            //get the cost data and dates
            List<double> y = new List<double>() { 1, 2, 3};
            DateTime second = new DateTime(2019, 02, 13);
            //-----------------------------

            var epochDates = forecast.generateUnixEpochFromDatetime(data.start, data.end, second.Subtract(data.start));

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);

            data.rSquared = rSquared;
            data.yIntercept = yIntercept;
            data.slope = slope;

            return data;
        }
    }
}