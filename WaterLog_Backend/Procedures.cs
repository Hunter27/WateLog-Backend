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
        ReadingsController readingsController;
        public Procedures(ReadingsEntry value, ReadingsController controller)
        {
            this.value = value;
            this.readingsController = controller;
        }

        public MonitorsController getMonitorsController()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();
            //string mySecret = config["Localhost:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer("Server=dev.retrotest.co.za;Database=iot;User Id=group1;Password=fNX^r+UKy3@CtYh5");
            //optionsBuilder.UseSqlServer("Server = localhost; Database = waterlog; User Id = test; Password = test123");
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
            optionsBuilder.UseSqlServer("Server=dev.retrotest.co.za;Database=iot;User Id=group1;Password=fNX^r+UKy3@CtYh5");
            //optionsBuilder.UseSqlServer("Server = localhost; Database = waterlog; User Id = test; Password = test123");
            //optionsBuilder.UseSqlServer(mySecret);
            DatabaseContext _context = new DatabaseContext(optionsBuilder.Options);

           SegmentsController _controller = new SegmentsController(_context, config);
            return _controller;

        }


        public async Task<int> getCorrespondingSensorAsync()
        {
            SegmentsController segController = getSegmentsController();
            IEnumerable<SegmentsEntry> allSegments = segController.Get().Result.Value;
            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;
            foreach (SegmentsEntry seg in allSegments)
            {
                if (seg.SenseIDIn == value.SenseID)
                {
                    segmentInid = seg.SenseIDIn;
                    segmentOutid = seg.SenseIDOut;
                    segmentid = seg.Id;
                    break;
                }

            }

     
            IEnumerable<ReadingsEntry> allReadings = readingsController.Get().Result.Value;
            allReadings = allReadings.OrderByDescending(read => read.TimesStamp);
            ReadingsEntry r1 = null, r2 = null;
            Boolean found1 = false, found2 = false;
            foreach (ReadingsEntry read in allReadings)
            {
                if(found1 && found2)
                {
                    break;
                }
                if (read.SenseID == segmentInid && found1 == false)
                {
                    r1 = read;
                    found1 = true;
                }
                if(read.SenseID == segmentOutid && found2 == false)
                {
                    r2 = read;
                    found2 = true;

                }

            }

            if (isLeakage(r1.Value, r2.Value)){
                //Updateleakagestatus
                await updateSegmentsEventAsync(segmentid,"leak",r1.Value,r2.Value);
            }
            else
            {
                //Updatewithoutleakagestatus
                await updateSegmentsEventAsync(segmentid,"normal",r1.Value,r2.Value);
            }



            return 7;
        }

        public async Task updateSegmentsEventAsync(int id, string status,double inv,double outv)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.UtcNow;
            entry.SegmentId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await controller.Post(entry);

        }

        public SegmentEventsController getSegmentsEventsController()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();
            //string mySecret = config["Localhost:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            //optionsBuilder.UseSqlServer("Server = localhost; Database = waterlog; User Id = test; Password = test123");
            optionsBuilder.UseSqlServer("Server=dev.retrotest.co.za;Database=iot;User Id=group1;Password=fNX^r+UKy3@CtYh5");
            //optionsBuilder.UseSqlServer(mySecret);
            DatabaseContext _context = new DatabaseContext(optionsBuilder.Options);

            SegmentEventsController _controller = new SegmentEventsController(_context, config);
            return _controller;
        }

        public Boolean isLeakage(double first,double second)
        {
            double margin = 2;
            if((first-second) > margin)
            {
                return true;
            }
            return false;
        }



    }
}
