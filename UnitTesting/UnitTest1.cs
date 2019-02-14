using NUnit.Framework;
using CheckValues;

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