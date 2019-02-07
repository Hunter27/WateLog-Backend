using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1;

namespace WaterLog_Backend
{
    public class Results
    {
        public int getLen()
        {

            List<string> lis = new List<string>();

            string mySecret = "Server=NOMBUSOSIBIYA;Database=waterlog;User Id=test;Password=test123";
            string queryString = "SELECT Type FROM dbo.SegmentEvents;";
            SqlConnection connection = new SqlConnection(mySecret);

            SqlCommand command = new SqlCommand(queryString, connection);
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {


                    lis.Add(reader[0].ToString());
                }
            }
            finally
            {

                reader.Close();
            }

            return lis.ToArray().Length;

        }

        public int getFirstID()
        {

            List<int> lis = new List<int>();
            string queryString = "SELECT Id FROM dbo.SegmentEvent;";
            string connectionString = "Server=NOMBUSOSIBIYA;Database=waterlog;User Id=test;Password=test123";
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(queryString, connection);
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    string val = reader[0].ToString();
                    lis.Add(Int32.Parse(val));
                }
            }
            finally
            {

                reader.Close();
            }

            return lis.ToArray()[0];

        }
    
}
}
