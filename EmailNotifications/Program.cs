using System;
using System.Collections.Generic;
using System.Text;

namespace EmailNotifications
{
    class Program
    {
        public static void Main(string[] args)
        {
            string[] inV = { "Segment 2","leak", "Severe", "2" ,"19 200", "2400", "40", "www.google.co.za" };
            Email em = new Email(inV);
            em.sendEmail();
        }
    }
}
