using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Web;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;

namespace EmailNotifications
{
    public class Email
    {
        static bool mailSent = false;

        string[] values;
        double totalcost, perhourcost;
        public Email(string[] emailValue)
        {
            values = emailValue;

        }
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

       

        public void sendEmail()
        {
            string[] styleProperties = { "color:red;padding-top: 40px;font-weight: bold", "color:red;font-weight:bold", "color:black;padding-top: 11px",
                                            "color:red;padding-top: 35px;font-weight: bold","color:black","color:black;padding-top: 20px",
                                               "color:grey" ,"color:grey","color:red;padding-top: 24px","color:red;","color:grey;padding-top: 9px"};
            string[] fontSizeProperties = {"6" ,"3","3","6","4","6","4","4","2","1","3" };
            string imgSrc = "logo.PNG";
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id",align:"center"))
            {
          
                table.StartBody();
                int count = 0;
                
                string[] items = { values[0] + " has a " + values[1], "("+ values[2]+")","This problem has been unresolved for " + values[3]+" days",
                                    "R " + values[4], "Has been lost",values[6]+"L or R "+ values[5]," is being lost!", "per hour","Resolve issue or precess","www.google.co.za","call third party help: 011111929292"};
                foreach (var alert in items)
                {
                    using (var tr = table.AddRow(classAttributes: "someattributes"))
                    {
                        if (count== 9)
                        {
                            tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count],url:true);
                        }
                        else
                        {
                            tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count]);

                        }
                       // Configuration.GetSection("LocalLiveDBConnectionString").Value)
                        count++;
                    


                    }
                }
                using (var tr = table.AddRow(classAttributes: "someattributes2"))
                {
                    tr.AddImage("testItem",align:"center", style: "padding-left: 130px", sizeX:"100",sizeY:"100");

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
                //string sFile = @"C:\Users\Nthokozo Motsumi\Documents\WaterLog\WaterLog-Back-End\EmailNotifications\logo.PNG";
                string current =  Directory.GetCurrentDirectory();
                string sFile = current + @"\logo.PNG";
                Attachment data = new Attachment(sFile, System.Net.Mime.MediaTypeNames.Application.Octet);
                data.ContentId = "testItem";
                message.Attachments.Add(data);
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
