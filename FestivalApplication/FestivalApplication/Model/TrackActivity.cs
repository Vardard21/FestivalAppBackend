using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class TrackActivity
    {
        public int ListID { get; set; }
        public int TrackID { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}
