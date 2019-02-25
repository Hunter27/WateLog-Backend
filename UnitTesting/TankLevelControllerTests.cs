using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;

namespace UnitTesting
{
    class TankLevelControllerTests
    {
        private IQueryable<TankLevelsEntry> mockData = new List<TankLevelsEntry>
        {
            new TankLevelsEntry
            {
                Id = 1, Percentage = 50, Instruction = "Turn off", LevelStatus = "Sufficient"
            },
            new TankLevelsEntry
            {
                Id = 2, Percentage = 20, Instruction = "Turn on", LevelStatus = "Not Sufficient"
            },

        }.AsQueryable();

        [TestCase(1)]
        public void TestGetById(int id)
        {
            var mockSet = new Mock<DbSet<TankLevelsEntry>>();
            mockSet.As<IOrderedQueryable<TankLevelsEntry>>().Setup(x => x.GetEnumerator())
                .Returns(mockData.GetEnumerator());
            var mockContext = new Mock<DatabaseContext>();
            mockContext.Setup(dbc => dbc.TankLevels).Returns(mockSet.Object);
            var tankLevelsController = new TankLevelsController(mockContext.Object, null);
            mockContext.Setup(x => x.TankLevels.FindAsync(id))
                .Returns(Task.FromResult(mockData.Where(x => x.Id == id).First()));
            var levels = tankLevelsController.Get(id).Result;
            Assert.IsNotNull(levels);
            Assert.AreEqual(levels.Value.Id, id);
            Assert.AreEqual(levels.Value.Instruction, "Turn off");
        }
    }
}
