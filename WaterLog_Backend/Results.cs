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
            string queryString = "SELECT Type FROM dbo.Monitors;";
            string connectionString = "Server=NTOKOZOMOTSUMI;Database=waterlog;Uid=test;Pwd=test123";
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(queryString, connection);
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {

                    // Console.WriteLine(String.Format("{0}, {1}",
                    //   reader[0], reader[1]));
                    lis.Add(reader[0].ToString());
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
            }

            return lis.ToArray().Length;
            
        }
    }
}
