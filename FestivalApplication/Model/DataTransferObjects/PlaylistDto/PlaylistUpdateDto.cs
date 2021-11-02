using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PlaylistUpdateDto
    {
        public int PlayListID { get; set; }
        public int TrackID { get; set; }
        public string TrackName { get; set; }
        public string TrackSource { get; set; }
        public double SongTime { get; set; }
        public bool Playing { get; set; }



    }
}