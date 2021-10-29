using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class ChatHistoryDto
    {
        public int StageID { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
