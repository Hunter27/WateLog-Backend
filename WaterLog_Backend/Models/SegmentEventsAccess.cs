using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class SegmentEventsAccess
    {
        DatabaseContext db;
        public SegmentEventsAccess(DatabaseContext db)
        {
            this.db = db;
            db = new DatabaseContext();
        }

        public IEnumerable<EventsList> ListEvents()
        {

            return db.SegmentLeaks.ToList().Select(s => new EventsList
            {

                OriginalTimeStamp = s.OriginalTimeStamp


            }).AsQueryable();

        }
    }
}
