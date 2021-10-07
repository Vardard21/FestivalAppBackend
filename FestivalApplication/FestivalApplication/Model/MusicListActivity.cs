using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class MusicListActivity
    {
        public int ID { get; set; }
        public int ListID { get; set; }
        public int StageID { get; set; }
        public int PreviousSong { get; set; }
        public int NextSong { get; set; }
        public int NextNextSong { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}
