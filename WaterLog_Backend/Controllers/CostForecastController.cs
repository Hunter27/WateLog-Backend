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

        [Route("daily")]
        [HttpGet]
        public async Task<ActionResult<LinearRegressionModel>> Get([FromBody] LinearRegressionModel data)
        {
            Forecast forecast = new Forecast();
            //get the cost data and dates
            Procedures proc = new Procedures(_db, _config);
            var results = await Task.Run(() => proc.CalculatePeriodWastageAsync(Procedures.Period.Daily).Result[0]);

            if(results.dataPoints.Count < 3)
            {
                throw new ArgumentException("data points should be at least 3");
            }
            var x = results.dataPoints.Select(row => row.x); //datetime data
            var y = results.dataPoints.Select(row => row.y); //cost data <double>

            DateTime start = x.ElementAt(0);
            DateTime second = x.ElementAt(1);
            DateTime end = x.Last();
            //-----------------------------

            var epochDates = forecast.generateUnixEpochFromDatetime(start, end, x.Count());

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);

            data.rSquared = rSquared;
            data.yIntercept = yIntercept;
            data.slope = slope;

            return data;
        }
    }
}
