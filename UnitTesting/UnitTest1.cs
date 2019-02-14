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
        public void TestCalculateDailyWastage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedDaily();
            var dataset = GetTestDataDaily();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            Assert.IsTrue(expected.Equals((proc.CalculateDailyWastage(dataset))[0]));
        }


        [Test]
        public void TestCalculateSeasonallyWastage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummer();
            var dataset = GetTestDataSummer();
            var empty = new List<SegmentEventsEntry>();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            Assert.IsTrue(expected.Equals((proc.CalculateSeasonallyWastage(dataset, empty, empty, empty))[0]));
        }
        [Test]
        public void TestsummaryUsageDaily()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummaryDaily();
            var dataset = GetTestDataSummaryDaily();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var i = proc.sumarryDailyUsage(dataset);
            Assert.IsTrue(expected.Equals((proc.sumarryDailyUsage(dataset))[0]));
        }
        [Test]
        public void TestsummaryUsageMonthly()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummaryMonthly();
            var dataset = GetTestDataSummaryMonthly();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var i = proc.summaryMonthlyUsage(dataset);
            Assert.IsTrue(expected.Equals((proc.summaryMonthlyUsage(dataset))[0]));
        }
        [Test]
        public void TestSummarySeasonallyUsage()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummarySummer();
            var dataset = GetTestDataSummarySummer();
            var empty = new List<SegmentEventsEntry>();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var i = proc.summarySeasonallyUsage(dataset, empty, empty, empty)[0];
            Assert.IsTrue(expected.Equals((proc.summarySeasonallyUsage(dataset, empty, empty, empty))[0]));
        }
        [Test]
        public void TestsummaryUsageCost()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummaryDailyCost();
            var dataset = GetTestDataSummaryDailyCost();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var i = proc.sumamryDailyCost(dataset);
            Assert.IsTrue(expected.Equals((proc.sumamryDailyCost(dataset))[0]));
        }
        [Test]
        public void TestsummaryMonthlyCost()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummaryMonthlyCost();
            var dataset = GetTestDataSummaryMonthly();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var i = proc.summaryMonthlyCost(dataset);
            Assert.IsTrue(expected.Equals((proc.summaryMonthlyCost(dataset))[0]));
        }
        [Test]
        public void TestSummarySeasonallyCost()
        {
            var build = new ConfigurationBuilder();
            IConfiguration _config = build.Build();
            var expected = GetTestExpectedSummarySummerCost();
            var dataset = GetTestDataSummarySummer();
            var empty = new List<SegmentEventsEntry>();
            Procedures proc = new Procedures(new DatabaseContext(), _config);
            var seasons = proc.summarySeasonallyUsage(dataset, empty, empty, empty);
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();


            var i = proc.summarySeasonsCost(seasons);
            Assert.IsTrue(expected.Equals(proc.summarySeasonsCost(seasons)));
        }
        private List<SegmentEventsEntry> GetTestDataSummarySummer()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "2/12/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst;
        }
        private List<SegmentEventsEntry> GetTestDataSummer()
        {
            List<SegmentEventsEntry> lst = new List<SegmentEventsEntry>();
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "2/12/2019 12:04:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 1, "1 / 1 / 2019 12:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 2, "1/1/2019 12:20:0 PM"));
            lst.Add(GetSegmentObject("leak", 240, 120, 2, "1/1/2019 12:50:15 PM"));
            lst.Add(GetSegmentObject("leak", 600, 300, 1, "1/1/2019 13:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 60, 3, "1/1/2019 13:20:00 PM"));
            lst.Add(GetSegmentObject("leak", 240, 0, 1, "1/1/2019 13:40:00 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 3, "1/1/2019 14:05:15 PM"));
            lst.Add(GetSegmentObject("leak", 60, 0, 1, "1/1/2019 14:10:15 PM"));
            lst.Add(GetSegmentObject("leak", 120, 0, 1, "1/1/2019 14:55:15 PM"));

            return lst;
        }


        private DataPoints<DateTime, double> GetTestExpectedSummarySummer()
        {
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("2/12/2019 12:04:00 PM")), 2.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:10:15 PM")), 30.0);
            return list;
        }
        private DataPoints<String, double> GetTestExpectedSummarySummerCost()
        {
            DataPoints<String, double> list = new DataPoints<String, double>();
            list.AddPoint("Summer", 1184.0);
            list.AddPoint("Winter", 0.0);
            list.AddPoint("Spring", 0.0);
            list.AddPoint("Autum", 0.0);
            return list;
        }

        private DataPoints<DateTime, double> GetTestExpectedSummer()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("2/12/2019 12:04:00 PM")), 2.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:10:15 PM")), 20.0);

            return list;
        }

        public DataPoints<DateTime, double> GetTestExpectedMonthly()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 8.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 4.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);
            return list;
        }
        public DataPoints<DateTime, double> GetTestExpectedSummaryMonthlyCost()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 444.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 592.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 148.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);

            return list;
        }
        public DataPoints<DateTime, double> GetTestExpectedSummaryMonthly()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 12.0);
            list.AddPoint((Convert.ToDateTime("2/1/2019 13:10:15 PM")), 16.0);
            list.AddPoint((Convert.ToDateTime("3/1/2019 14:05:15 PM")), 4.0);
            list.AddPoint((Convert.ToDateTime("2000/4/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/5/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/6/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/7/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/8/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/9/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/10/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/11/1")), 0.0);
            list.AddPoint((Convert.ToDateTime("2000/12/1")), 0.0);

            return list;
        }
        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryMonthly()
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

        public DataPoints<DateTime, double> GetTestExpectedSummaryDailyCost()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 444.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 592.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 148.0);

            return list;
        }

        public DataPoints<DateTime, double> GetTestExpectedSummaryDaily()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 12.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 16.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 4.0);

            return list;
        }
        public DataPoints<DateTime, double> GetTestExpectedDaily()
        {
            //Build data
            DataPoints<DateTime, double> list = new DataPoints<DateTime, double>();
            list.AddPoint((Convert.ToDateTime("1/1/2019 12:04:00 PM")), 8.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 13:10:15 PM")), 10.0);
            list.AddPoint((Convert.ToDateTime("1/1/2019 14:05:15 PM")), 4.0);

            return list;
        }

        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryDailyCost()
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
        public List<IGrouping<int, SegmentEventsEntry>> GetTestDataSummaryDaily()
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

        public SegmentEventsEntry GetSegmentObject(string eve, double inn, double outt, int segid, string time)
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