using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WaterLog_Backend.Models;
using WaterLog_Backend.Controllers;
using System.Threading.Tasks;


namespace EmailNotifications
{
    class Program
    {
        static bool mailSent = false;
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }

       

        static void Main(string[] args)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id",align:"center"))
            {
          
                table.StartBody();
                string[] items= { "1", "2", "3", "4" };
                foreach (var alert in items)
                {
                    using (var tr = table.AddRow(classAttributes: "someattributes"))
                    {
                        tr.AddCell(alert,align:"center",fontColor:"red");
                        TableStructure.Row rr = new TableStructure.Row(sb, id: "some-id");
                        rr.Dispose();


                    }
                }
                table.EndBody();
            }
            string outS = sb.ToString();
            
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("nmotsumi@retrorabbit.co.za", "****"),
                    EnableSsl = true
                };
                MailAddress from = new MailAddress("nmotsumi@retrorabbit.co.za", "Ntokozo Motsumi");
                MailAddress to = new MailAddress("ntokozo.motsumi@gmail.com", "Ntokozo Motsumi");
                MailMessage message = new MailMessage(from, to);
                
                message.Subject = "Send Using Web Mail";

                // SEND IN HTML FORMAT (comment this line to send plain text).
                message.IsBodyHtml = true ;
                message.Body = outS;
                client.Send(message);

    

                // TODO: Replace with the name of your remote SMTP server.
             
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
