using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;


namespace WaterLog_Backend
{
    public class Results
    {
  

    public int getLen()
        {
            
            List<string> lis = new List<string>();
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();
            var config = builder.Build(); 
            string mySecret = "Server=dev.retrotest.co.za;Database=iot;User Id=group1;Password=fNX^r+UKy3@CtYh5";
            string queryString = "SELECT Type FROM dbo.Monitors;";
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
            string queryString = "SELECT Id FROM dbo.Monitors;";
            string connectionString = "Server=dev.retrotest.co.za;Database=iot;User Id=group1;Password=fNX^r+UKy3@CtYh5";
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
