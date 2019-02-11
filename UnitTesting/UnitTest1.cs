using NUnit.Framework;
using CheckValues;
using System.Linq;
using WaterLog_Backend.Models;
using WaterLog_Backend;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void FirstIdMonitor()
        {
            Results val = new Results();
            Assert.AreEqual(1, val.getFirstID());
        }

        [Test]
        public void TestCalculateDailyWastage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedDaily();
            var dataset = GetTestDataDaily();
            Procedures proc = new Procedures(new DatabaseContext(),_config);

            Assert.IsTrue(expected.Equals((proc.CalculateDailyWastage(dataset))[0]));
        }

        [Test]
        public void TestCalculateMonthlyWastage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedMonthly();
            var dataset = GetTestDataMonthly();
            Procedures proc = new Procedures(new DatabaseContext(), _config);

            Assert.IsTrue(expected.Equals((proc.CalculateMonthlyWastage(dataset))[0]));
        }

        [Test]
        public void TestCalculateSeasonallyWastage()
        {
            
        }

        public DataPoints<DateTime, double> GetTestExpectedMonthly()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 8.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 4.0);

            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataMonthly()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "2/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "2/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "2/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "3/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "3/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "3/1/2019 14:55:15 PM"));

            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();
        }

        public DataPoints<DateTime,double> GetTestExpectedDaily()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")),8.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 4.0);

            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataDaily()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst.GroupBy(a => a.TimeStamp.Hour).ToList();    
        }

        public SegmentEventsEntry GetSegmentObject(string eve,double inn, double outt,int segid,string time)
        {
            SegmentEventsEntry obj = new SegmentEventsEntry();
            obj.EventType = eve;
            obj.FlowIn = inn;
            obj.FlowOut = outt;
            obj.SegmentsId = segid;
            obj.TimeStamp = Convert.ToDateTime(time);

            return obj;
        }
    }
}