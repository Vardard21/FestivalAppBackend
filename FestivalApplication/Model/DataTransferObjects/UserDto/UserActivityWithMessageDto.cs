using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class UserActivityWithMessageDto
    {
        public int StageID { get; set; }
        public DateTime Entry { get; set; }
        public DateTime? Exit { get; set; }
        public List<MessageShortDto> MessageHistory { get; set; }
    }
}
