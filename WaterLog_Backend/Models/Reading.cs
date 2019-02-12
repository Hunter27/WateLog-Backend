using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class Reading
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public DateTime TimesStamp { get; set; } 
        public int MonitorId { get; set; }
        public virtual Monitor Monitor { get; set; }

    }
}
