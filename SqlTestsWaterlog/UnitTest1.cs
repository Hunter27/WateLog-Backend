
using NUnit.Framework;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
namespace Tests
{
    public class Tests
    {
        MonitorsController monitorsController;
        MonitorsContext monitorsContext;

        int monitorsId;
        [SetUp]
        public void Setup()
        {
            monitorsContext = new MonitorsContext();
            monitorsController = new MonitorsController(monitorsContext);


            //Pass contact ID and store the retrieved contact ID

            monitorsId = monitorsController.Get(1).Id;


        }

        [Test]
        public void GetContact()
        {

            Assert.IsTrue(monitorsId.Equals(1));

        }
    }
}