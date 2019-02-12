using NUnit.Framework;
using CheckValues;
using WaterLog_Backend.Models;
using System;
using System.Collections.Generic;
using WaterLog_Backend;

namespace Tests
{
    public class Tests
    {
        /*
        [Test]
        public void FirstIdMonitor()
        {
            Results val = new Results();
            Assert.AreEqual(1, val.getFirstID());
        }*/

        [Test]
        public void given_start_second_end_dates_generate_correct_number_of_epochDates()
        {
            Forecast f = new Forecast();
            DateTime start = new DateTime(2019, 02, 12);
            DateTime second = new DateTime(2019, 02, 13);
            DateTime end = new DateTime(2019, 02, 15);
            var arr = f.generateUnixEpochFromDatetime(start, end, second.Subtract(start));

            Assert.AreEqual(4, arr.Count);
        }

        /* TODO: test for an exception
        [Test]
        public void given_different_length_y_and_x_values_throw_exception()
        {
            Forecast f = new Forecast();
            DateTime start = new DateTime(2019, 02, 12);
            DateTime second = new DateTime(2019, 02, 13);
            DateTime end = new DateTime(2019, 02, 15);
            var x = f.generateUnixEpochFromDatetime(start, end, second.Subtract(start));
            List<double> y = new List<double>() { 1, 2 };
            double rS, yInt, slope;

            Assert.(f.LinearRegression(x.ToArray(), y.ToArray(), out rS, out yInt, out slope));
        }*/
    }
}