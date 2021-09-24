using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class StageUpdateDto
    {
        public int StageID { get; set; }
        public bool StageActive { get; set; }
    }
}
