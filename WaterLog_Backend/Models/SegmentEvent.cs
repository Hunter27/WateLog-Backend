using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SegmentEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public DateTime TimeStamp { get; set; }
        public double FlowIn { get; set; }
        public double FlowOut { get; set; }       
        public int SegmentId { get; set; }
        public virtual Segment Segment { get; set; }
    }
}
