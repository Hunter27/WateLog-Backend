﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using WaterLog_Backend;
using WebApplication1;
using WaterLog_Backend.Models;
using WaterLog_Backend.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CheckValues
{
    public class Results
    {
        public MonitorsController getController()
        {
            string connection = "";
            var _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();

            if (config.GetSection("LocalLiveDBConnectionString").Exists())
            {
                connection = config.GetSection("LocalLiveDBConnectionString").Value;
            }
            else
            {
                connection = _config.GetSection("LocalLiveDBConnectionString").Value;
            }

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer(connection);
            DatabaseContext _context = new DatabaseContext(optionsBuilder.Options);
            MonitorsController _controller = new MonitorsController(_context, config);
            return _controller;
        }

        public void item()
        {
            MonitorsController _controller = getController();
            Task<ActionResult<MonitorsEntry>> item = _controller.Get(1);
            ActionResult<MonitorsEntry> item2 = item.Result;
            MonitorsEntry item3 = item2.Value;
            Console.WriteLine(item3.Id);

        }
        public int getLen()
        {
            MonitorsController _controller = getController();
            Task<ActionResult<IEnumerable<MonitorsEntry>>> item = _controller.Get();
            ActionResult<IEnumerable<MonitorsEntry>> item2 = item.Result;
            IEnumerable<MonitorsEntry> item3 = item2.Value;
            int count = 0;
            foreach (MonitorsEntry itemv in item3)
            {
                count++;
            }
            return count;
        }

        public int getFirstID()
        {
            MonitorsController _controller = getController();
            Task<ActionResult<MonitorsEntry>> item = _controller.Get(1);
            ActionResult<MonitorsEntry> item2 = item.Result;
            MonitorsEntry item3 = item2.Value;
            return item3.Id;
        }
    }
}
