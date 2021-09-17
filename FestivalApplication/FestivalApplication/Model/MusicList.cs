using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class MusicList
    {
        public int ListID { get; set; }
        public string ListName { get; set; }
        public List<TrackActivity> MusicTracks { get; set; }
        public List<MusicListActivity> PlayList { get; set; }
        public Boolean Playing { get; set; }
        public int CurrentTrack { get; set; }
    }
}
