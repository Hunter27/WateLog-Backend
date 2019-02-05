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
           

            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("nmotsumi@retrorabbit.co.za", "*****"),
                    EnableSsl = true
                };
                MailAddress from = new MailAddress("nmotsumi@retrorabbit.co.za", "Ntokozo Motsumi");
                MailAddress to = new MailAddress("dadams@retrorabbit.co.za", "Darren Adams");
                MailMessage message = new MailMessage(from, to);
                
                message.Subject = "Send Using Web Mail";

                // SEND IN HTML FORMAT (comment this line to send plain text).
                message.IsBodyHtml = true ;
                message.Body = "<HTML><BODY><B>Hello World!</B></BODY></HTML>";
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
