using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class ActionableEvent
    {
        public int Id { get; set; }
        public string Severity { get; set; }
        public DateTime OriginalTimeStamp { get; set; }
        public DateTime LatestTimeStamp { get; set; }
        public string Status { get; set; }
        public int SegmentId { get; set; }

        public Segment Segment { get; set; }
    }
}
