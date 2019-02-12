using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class LinearRegressionModel
    {
        public double yIntercept { get; set; }
        public double slope { get; set; }
        public double rSquared { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }
}
