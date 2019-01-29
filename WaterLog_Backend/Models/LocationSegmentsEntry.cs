using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class LocationSegmentsEntry
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public LocationsEntry LocationsEntry { get; set; }
        public int SegmentId { get; set; }
        [ForeignKey("SegmentId")]
        public SegmentsEntry segmentsEntry { get; set; }
    }

       
    }

