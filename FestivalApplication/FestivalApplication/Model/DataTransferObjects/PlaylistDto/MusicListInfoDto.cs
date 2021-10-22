using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class MusicListInfoDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<PlaylistRequestDto> PlaylistTracks { get; set; }
    }
}
