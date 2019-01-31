
using NUnit.Framework;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using SqlTestsWaterlog;

namespace Tests
{
    public class Tests
    {
        TestController monitorsController;
        MonitorsContext monitorsContext;
        IConfiguration _config;
        int monitorsId;
        int first, last;
        string _setup;
        string queryString;
        SqlConnection connection;
        SqlCommand command;
        SqlDataReader reader;
        List<string> lis =  new List<string>() ;
        

        
        /*public void GetMonitor(int id)
        {
            Task<ActionResult<MonitorsEntry>> val = monitorsController.Get(id);
            MonitorsEntry val2 = val.Result.Value;
            int num = val2.Id;
            Assert.IsTrue(val2.Equals(1));

        }

        [Test]
        public void GetContact2()
        {
            Task<ActionResult<MonitorsEntry>> val = monitorsController.Get(1);
            MonitorsEntry val2 = val.Result.Value;
            int num = val2.Id;
            Assert.IsTrue(val2.Equals(1));

        }*/



    }
}