
using System;
using Xunit;
using WaterLog_Backend;



namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {


        }
        [Fact]
        public void CountMonitor()
        {
            Results val = new Results();
            Assert.Equal(3, val.getLen());


        }
    }
}
