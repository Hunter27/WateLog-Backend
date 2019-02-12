using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Monitor
    {
        public Monitor()
        {
            Initialize();
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public double Max_flow { get; set; }
        public string Status { get; set; }

        public ICollection<Reading> Reading { get; set; }
        public virtual Location Location { get; set; }
        public virtual Segment Segment { get; set; }

        public void Initialize()
        {
            Reading = new HashSet<Reading>();
        }

       
    }
}
