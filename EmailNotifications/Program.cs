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
            string[] styleProperties = { "color:red;padding-top: 40px", "color:red;font-weight:bold", "color:black;padding-top: 11px",
                                            "color:red;padding-top: 35px","color:black","color:black;padding-top: 20px",
                                               "color:grey" ,"color:grey","color:red;padding-top: 24px","color:red;","color:grey;padding-top: 9px"};
            string[] fontSizeProperties = {"10" ,"3","3","10","4","9","5","5","3","2","3" };

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id",align:"center"))
            {
          
                table.StartBody();
                int count = 0;
                string[] items= { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"};
                foreach (var alert in items)
                {
                    using (var tr = table.AddRow(classAttributes: "someattributes"))
                    {
                        tr.AddCell(alert,align:"center",style: styleProperties[count], fontSize:fontSizeProperties[count]);
                        count++;
                    


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
