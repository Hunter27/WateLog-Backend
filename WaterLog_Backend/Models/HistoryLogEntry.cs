using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class HistoryLogEntry
    {
        public int Id { get; set; }
        public EnumTypeOfEvent Type { get; set; }
        public DateTime Date { get; set; }
        public int EventsId { get; set; }
       
    }
}
