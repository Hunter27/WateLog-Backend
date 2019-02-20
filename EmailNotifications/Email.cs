using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Web;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;
using WaterLog_Backend.Models;

namespace EmailNotifications
{
    public class Email
    {
        static bool mailSent = false;
        string[] values;
        IConfiguration _conf;
        public Email(string[] emailValue,IConfiguration config)
        {
            values = emailValue;
            _conf = config;        
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

        public void SendMail(Recipient[] recipient)
        {
            if (recipient != null)
            {
                string email = ConstructEmail();
                if (email == "ERROR")
                {
                    throw new Exception("Error : No Values Given");
                }
                else if (recipient.Length < 1)
                {
                    throw new Exception("Error : No Address Given");
                }
                else
                {
                    try
                    {
                        var client = new SmtpClient("smtp.gmail.com", 587)
                        {
                            //TODO: Move email address to config file
                            Credentials = new NetworkCredential("nmotsumi@retrorabbit.co.za", _conf.GetSection("Password").Value),
                            EnableSsl = true
                        };

                        //Set From
                        MailAddress from = new MailAddress(_conf.GetSection("Sender").Value, _conf.GetSection("SenderName").Value);

                        //Set To
                        MailAddress to = new MailAddress(recipient[0].Address, recipient[0].Name);
                        MailMessage message = new MailMessage(from, to);
                        for (int i = 1; i < recipient.Length; i++)
                        {
                            message.CC.Add(new MailAddress(recipient[i].Address, recipient[i].Name));
                        }

                        message.Subject = "WaterLog Notification";
                        message.IsBodyHtml = true;
                        message.Body = email;
                        client.Send(message);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("{0} Exception caught.", e);
                    }
                }
            }
            else
            {
                throw new Exception("ERROR : Recipients Cannot Be Null");
            }
        }

        public string ConstructEmail()
        {
            if (values.Length > 0)
            {
                string[] styleProperties = {
                "color:red;padding-top: 40px;",
                "color:red;",
                "color:black;padding-top: 11px;",
                "color:red;padding-top: 35px;",
                "color:black;",
                "color:black;padding-top: 20px;",
                "color:grey;" ,
                "color:grey;",
                "color:red;padding-top: 24px;",
                "color:red;",
                "color:grey;padding-top: 9px;" };
                string[] fontSizeProperties = { "6", "3", "3", "6", "4", "6", "4", "4", "4", "4", "2" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id", align: "center"))
                {
                    table.StartBody();
                    int count = 0;
                    string[] items =
                    {
                        "<b>" + values[0] + " has a " + values[1] + "</b>",
                        "<b>(" + values[2]+")</b>",
                        "This problem has been <b>unresolved for " + values[3]+" days</b>",
                        "<b>R " + values[4] + "</b>",
                        "<b>Has been lost!</b>",
                        "<b>" +values[6]+"L or R "+ values[5] + "</b>",
                        " is currently being lost", "per hour",""
                        ,values[7],
                        "call third party help: <u>&zwj;011111929292</u>"
                    };
                    foreach (var alert in items)
                    {
                        using (var tr = table.AddRow(classAttributes: "someattributes"))
                        {
                            if (count == 9)
                            {
                                tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count], url: true);
                            }
                            else
                            {
                                tr.AddCell(alert, align: "center", style: styleProperties[count], fontSize: fontSizeProperties[count]);
                            }
                            count++;
                        }
                    }
                    using (var tr = table.AddRow(classAttributes: "someattributes2"))
                    {
                        tr.AddImage("https://res.cloudinary.com/retro-rabbit/image/upload/v1549634899/logo.png", align: "center", style: "padding-left: 130px", sizeX: "100", sizeY: "100");

                    }
                    table.EndBody();
                }

                return sb.ToString();
            }
            else
            {
                //Something went wrong
                return "ERROR";
            }
        }
    }
}
