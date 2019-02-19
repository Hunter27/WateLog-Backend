using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    //This class acts as a way to structure queries from the database and provide to the front end.
    public class GetAlerts
    {
        public DateTime Date;
        public string EntityName;
        public int EntityId;
        public string EntityType;
        public double Cost;
        public string Severity;
        //Litres related to the Type IE- Leak
        public double TypeLitres;
        //Total Litres used throughout the system during the event timeline
        public double TotalLitres;

        public GetAlerts(DateTime d, string en, int eid, string et, double c, string s, double tpl,double ttl)
        {
            Date = d;
            EntityName = en;
            EntityType = et;
            EntityId = eid;
            Cost = c;
            Severity = s;
            TypeLitres = tpl;
            TotalLitres = ttl;
        }
    }
}
