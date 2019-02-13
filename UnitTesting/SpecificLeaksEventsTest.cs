using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WaterLog_Backend;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
using WebApplication1;

namespace UnitTesting
{
    class SpecificLeaksEventsTest
    {
        private readonly HttpClient _client;
        private readonly DatabaseContext _context;
        /*
        IControllerService _service;
        SegmentLeaksController _controller;
        public SpecificLeaksEventsTest() {

            _service = new 
        }*/

        /*public SpecificLeaksEventsTest()
        {
            // Set up server configuration
            var configuration = new ConfigurationBuilder()
            // Indicate the path for our source code
            .SetBasePath(Directory.GetCurrentDirectory())
            .Build();
            // Create builder
            var builder = new WebHostBuilder()
            // Set test environment
            .UseEnvironment("Testing")
            .UseStartup<Startup>()
            .UseConfiguration(configuration);
            // Create test server
            var server = new TestServer(builder);
            // Create database context
            this._context = server.Host.Services.GetService(typeof(DatabaseContext)) as DatabaseContext;
            // Create client to query server endpoints
            this._client = server.CreateClient();
        }*/

        private IQueryable<SegmentLeaksEntry> mockData = new List<SegmentLeaksEntry>
        {
            new SegmentLeaksEntry
            {
                Id = 1, SegmentsId = 1, Severity = "High", ResolvedStatus = "Unresolved",
                LatestTimeStamp = DateTime.Now, OriginalTimeStamp = DateTime.Today.Date
            },

            new SegmentLeaksEntry
            {
                Id = 2, SegmentsId = 1, Severity = "Low", ResolvedStatus = "Resolved",
                LatestTimeStamp = DateTime.Now, OriginalTimeStamp = DateTime.Today.Date
            }
        }.AsQueryable();
        
        [TestCase(1)]
        public async Task getvalue(int id) {
            var mockSet = new Mock<DbSet<SegmentLeaksEntry>>();
            mockSet.As<IQueryable<SegmentLeaksEntry>>().Setup(m => m.GetEnumerator()).Returns(mockData.GetEnumerator());
            var mockContext = new Mock<DatabaseContext>();
            mockContext.Setup(s => s.SegmentLeaks).Returns(mockSet.Object);
            var LeaksController = new SegmentLeaksController(mockContext.Object, null, null);

            int before = mockContext.Invocations.Count;
            var SegmentHistory = LeaksController.GetSegmentHistory(id);
            int after = mockContext.Invocations.Count;
            int insideFunction = 1;
            Assert.AreEqual(before, after - insideFunction);

            //mockContext.Verify(x => x.SegmentLeaks, Times.Once);
        }
    }
}
