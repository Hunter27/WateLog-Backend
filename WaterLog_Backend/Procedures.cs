using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
using WebApplication1;

namespace WaterLog_Backend
{
    public class Procedures
    {
        ReadingsEntry value;
        ReadingsController controller;
        public Procedures(ReadingsEntry value, ReadingsController controller)
        {
            this.value = value;
            this.controller = controller;
        }

        public MonitorsController getMonitorsController()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();
            //string mySecret = config["Localhost:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer("Server = NTOKOZOMOTSUMI; Database = waterlog; User Id = test; Password = test123");
            //optionsBuilder.UseSqlServer(mySecret);
            DatabaseContext _context = new DatabaseContext(optionsBuilder.Options);

            MonitorsController _controller = new MonitorsController(_context, config);
            return _controller;

        }

        public SegmentsController getSegmentsController()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();
            //string mySecret = config["Localhost:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer("Server = NTOKOZOMOTSUMI; Database = waterlog; User Id = test; Password = test123");
            //optionsBuilder.UseSqlServer(mySecret);
            DatabaseContext _context = new DatabaseContext(optionsBuilder.Options);

           SegmentsController _controller = new SegmentsController(_context, config);
            return _controller;

        }


        public int getCorrespondingSensor()
        {
            SegmentsController segController = getSegmentsController();
            IEnumerable<SegmentsEntry> allSegments = segController.Get().Result.Value;
            foreach (SegmentsEntry seg in allSegments)
            {
                if (seg.SenseIDIn == value.Id)
                {
                    Console.WriteLine("found in");
                }
                else
                {
                    Console.WriteLine("found out");
                }

            }


            return 7;
        }

        public void updateSegmentsEvent()
        {


        }



    }
}
