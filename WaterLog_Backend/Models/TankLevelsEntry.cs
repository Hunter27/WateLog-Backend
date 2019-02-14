using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class TankLevelsEntry
    {
        [Key]
        public int TankId { get; set; }
        public int Percentage { get; set; }
        public string LevelStatus { get; set; }
        public string Instruction { get; set; }
        
    }
}
