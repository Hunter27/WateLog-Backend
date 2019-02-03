using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class ReadingsEntry
    {
        public int Id { get; set; }

        public int SenseID { get; set; }
        [ForeignKey("SenseID")]
        public MonitorsEntry MonitorsEntry { get; set; }
        public double Value { get; set; }
        public DateTime TimesStamp { get; set; }

       
    }
}
