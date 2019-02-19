﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SegmentEventsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public SegmentEventsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentEventsEntry>>> Get()
        {
            return await _db.SegmentEvents.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentEventsEntry>> Get(int id)
        {
            var segment = await _db.SegmentEvents.FindAsync(id);

            if (segment == null)
            {
                return NotFound();
            }
            return segment;
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] SegmentEventsEntry value)
        {
            try { 
            await _db.SegmentEvents.AddAsync(value);
            await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        [Route("dailywastage")]
        public async Task<DataPoints<DateTime,double>> GetDailyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Daily);
            return ret.FirstOrDefault();
        }

        [Route("monthlywastage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }

        [Route("seasonallywastage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.CalculatePeriodWastageAsync(Procedures.Period.Seasonally);
        }

        [Route("dailyUsage")]
        public async Task<DataPoints<DateTime, double>> GetDailyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Daily);
            return ret.FirstOrDefault();
        }
        [Route("monthlyUsage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyUsageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }
        [Route("seasonallyUsage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.SummaryPeriodUsageAsync(Procedures.Period.Seasonally);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] SegmentEventsEntry value)
        {
            try { 
            var old = await _db.SegmentEvents.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.SegmentEvents.FindAsync(id);
            _db.SegmentEvents.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
