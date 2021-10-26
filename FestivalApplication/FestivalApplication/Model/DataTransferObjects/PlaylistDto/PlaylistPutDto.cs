using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PlaylistPutDto
    {
        public int PlayListId { get; set; }
        public List<TrackPositionDto> TrackPosition { get; set; }




    }
}