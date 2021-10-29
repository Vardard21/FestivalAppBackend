using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class StageCreateDto
    {
        public string StageName { get; set; }
        public bool StageActive { get; set; }
        public string StageGenre { get; set; }
        public string StageRestriction { get; set; }
    }
}
