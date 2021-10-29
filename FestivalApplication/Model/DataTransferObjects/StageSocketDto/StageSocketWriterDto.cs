using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class StageSocketWriterDto<o>
    {
        public string StageCase { get; set; }
        public Response<o> StageData { get; set; }
    }
}
