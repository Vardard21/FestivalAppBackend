using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PlaylistReceiveDto
    {
        public string ReceivedCase { get; set; }
        public int StageID { get; set; }
        public int TrackID { get; set; }
        public int PlaylistID { get; set; }
        public double SongTime { get; set; }


    }
}
