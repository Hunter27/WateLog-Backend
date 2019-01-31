
using System;
using Xunit;
using WaterLog_Backend;
using Microsoft.AspNetCore;

namespace XUnitTestProject1
{
    
    public class UnitTest1
    {
        

    [Fact]
        public void CheckNotEmptyMonitor()
        {
            Results val = new Results();
            Assert.NotEqual(0, val.getLen());


        }

        [Fact]
        public void FirstIdMonitor()
        {
            Results val = new Results();
            Assert.Equal(1, val.getFirstID());


        }
    }
}
