using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Filter
    {
        public int Segment { get; set; }
        public int SensorType { get; set; }
        public int SensorId { get; set; }
        public string Severity { get; set; }
    }
}
