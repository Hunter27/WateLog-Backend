using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class MonitorsContext : DbContext
    {
        public DbSet<MonitorsEntry> Monitors { get; set; }
    }
}
