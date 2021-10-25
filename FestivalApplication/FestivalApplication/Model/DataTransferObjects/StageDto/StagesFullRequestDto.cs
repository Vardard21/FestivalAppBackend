using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class StagesFullRequestDto
    {
        public int StageID { get; set; }
        public string StageName { get; set; }
        public string CurrentSong { get; set; }
        public string StageGenre { get; set; }
        public string StageRestriction { get; set; }
        public int NumberOfUsers { get; set; }
        public Boolean StageActive { get; set; }
    }
}
