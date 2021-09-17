using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class MusicListActivity
    {
        public int ListID { get; set; }
        public int StageID { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}
