using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Filter
    {
        //public int id { get; set; }
        public int Segment { get; set; }
        public int SensorType { get; set; }
        public int SensorId { get; set; }
        public Range Severity { get; set; }
        public  enum Range : int { low = 1, medium = 2, high = 3 };
    }
}
