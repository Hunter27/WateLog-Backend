using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace EmailNotifications
{
    public class Email
    {
        static bool mailSent = false;
        string[] values;
        public Email(string[] emailValue)
        {
            values = emailValue;
        }
     
        public void sendEmail()
        {
            string[] styleProperties = {
                "color:red;padding-top: 40px",
                "color:red;font-weight:bold",
                "color:black;padding-top: 11px",
                "color:red;padding-top: 35px",
                "color:black",
                "color:black;padding-top: 20px",
                "color:grey" ,
                "color:grey",
                "color:red;padding-top: 24px",
                "color:red;",
                "color:grey;padding-top: 9px" };
            string[] fontSizeProperties = {"6" ,"3","3","6","4","6","4","4","2","1","3" };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (TableStructure.Table table = new TableStructure.Table(sb, id: "some-id",align:"center"))
            {         
                table.StartBody();
                int count = 0;                
                string[] items = { values[0] + "is " + values[1], "("+ values[2]+")","This problem has been unresolved for " + values[3]+" days",
                                    "R " + values[4], "Has been lost","R"+ values[5]," is being lost!", "per hour","Resolve issue or precess","Here","call third party help: 011111929292"};
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
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
