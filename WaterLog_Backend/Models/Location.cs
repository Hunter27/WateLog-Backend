using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Location
    {
        public int Id { get; set; }
        
        public double Long { get; set; }
        public double Lat { get; set; }
        public int MonitorId { get; set; }
        public virtual Monitor Monitor { get; set; }


    }
}
