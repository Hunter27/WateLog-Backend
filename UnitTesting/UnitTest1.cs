using NUnit.Framework;
using CheckValues;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void CheckNotEmptyMonitor()
        {
            Results val = new Results();
            Assert.AreNotEqual(0, val.getLen());


        }

        [Test]
        public void FirstIdMonitor()
        {
            Results val = new Results();
            Assert.AreEqual(1, val.getFirstID());


        }
    }
}