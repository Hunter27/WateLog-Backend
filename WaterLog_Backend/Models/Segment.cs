using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Segment
    {
        public Segment()
        {
            Initialize();
        }
        
        public int Id { get; set; }

        public int Monitor1Id { get; set; }
        public int Monitor2Id { get; set; }

        public Monitor Monitor { get; set; }

        public ICollection<SegmentEvent> SegmentEvent { get; set; }
        public ICollection<ActionableEvent> ActionableEvent { get; set; }

        public void Initialize()
        {
            SegmentEvent = new HashSet<SegmentEvent>();
            ActionableEvent = new HashSet<ActionableEvent>();
        }


    }
}
