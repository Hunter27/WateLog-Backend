using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<LinearRegressionModel>> GenerateLinearCostForecast()
        {
            Forecast forecast = new Forecast();
            //get the cost data and dates
            Procedures proc = new Procedures(_db, _config);
            var results = (await proc.SummaryPeriodCostsAsync(Procedures.Period.Daily)).FirstOrDefault();

            if (results.dataPoints.Count < 3)
            {
                return new BadRequestObjectResult("Not Possible"); ;
            }
            var x = results.dataPoints.Select(row => row.x); //datetime data
            var y = results.dataPoints.Select(row => row.y); //cost data <double>

            DateTime start = x.ElementAt(0);
            DateTime second = x.ElementAt(1);
            DateTime end = x.Last();

            var epochDates = forecast.generateUnixEpochFromDatetime(start, end, x.Count());

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);

            LinearRegressionModel data = new LinearRegressionModel();
            data.rSquared = rSquared;
            data.yIntercept = yIntercept;
            data.slope = slope;
            data.start = epochDates.First();
            data.end = epochDates.Last();
            data.numOfElements = epochDates.Count();

            return data;
        }

        [Route("monthly/{id}")]
        [HttpGet]
        public async Task<ActionResult<double>> GetMonthCostForecast(int id)
        {
            Procedures P = new Procedures();
            var thisMonthsEvents = P.sumamryDailyCost( await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == id && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync()).FirstOrDefault();

            if (thisMonthsEvents.dataPoints.Count < 3)
            {
                return new BadRequestObjectResult("Not Possible"); ;
            }
            var orderedEvents = thisMonthsEvents.dataPoints.OrderBy(d => d.x.Date);
            var x = (orderedEvents.ToArray()).Select(row => row.x); //datetime data
            var y = (orderedEvents.ToArray()).Select(row => row.y); //cost data <double>

            DateTime start = x.ElementAt(0);
            DateTime second = x.ElementAt(1);
            DateTime end = x.Last();

            Forecast forecast = new Forecast();
            var epochDates = forecast.generateUnixEpochFromDatetime(start, end, y.Count());

            double rSquared, yIntercept, slope;

            forecast.LinearRegression(epochDates.ToArray(), y.ToArray(), out rSquared, out yIntercept, out slope);
            var day = DateTime.DaysInMonth(DateTime.Now.Year, id);
            var date = new DateTime(DateTime.Now.Year, id, day);
            
            return slope*(new DateTimeOffset(date).ToUnixTimeSeconds()) + yIntercept;
        }

        [Route("seasonal")]
        [HttpGet]
        public async Task<ActionResult<LinearRegressionModel>> GenerateLinearSeasonallyCostForecast()
        {
            Forecast forecast = new Forecast();
            //get the cost data and dates
            Procedures proc = new Procedures(_db, _config);
            var results = proc.CalculatePeriodWastageAsync(Procedures.Period.Seasonally);

            LinearRegressionModel data = new LinearRegressionModel();
            data.rSquared = 5;
            data.yIntercept = 1;
            data.slope = 3;

            return data;
        }
    }
}
