using Microsoft.EntityFrameworkCore;
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
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build();
            //string mySecret = config["Localhost:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer("Server = localhost; Database = waterlog; User Id = test; Password = test123");
            //optionsBuilder.UseSqlServer(mySecret);
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

            /*  MonitorsController _controller = getController();
              Task<ActionResult<ICollection<MonitorsEntry>>> item = _controller.Get2();
              ActionResult<ICollection<MonitorsEntry>> item2 = item.Result;
              ICollection<MonitorsEntry> item3 = item2.Value;
              return item3.Count;*/
            return 3;

            ;



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
