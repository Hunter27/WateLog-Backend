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
        public int Tank_Id { get; set; }
        public int Percentage { get; set; }
        public string Level_Status { get; set; }
        public string Instruction { get; set; }
        
    }
}
