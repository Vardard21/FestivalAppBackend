using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class TrackItemDto
    {
        public int TrackID { get; set; }
        public string TrackName { get; set; }
        public string TrackSource { get; set; }
    }
}
