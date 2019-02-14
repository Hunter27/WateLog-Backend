using NUnit.Framework;
using CheckValues;
using WaterLog_Backend.Models;
using System;
using WaterLog_Backend;

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
    }
}