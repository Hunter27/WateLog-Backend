using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class EventsList
    {
        public DateTime OriginalTimeStamp { get; set; }
        public SegmentsEntry SegmentsEntry { get; set; }
        public string Severity { get; set; }
    }
}
