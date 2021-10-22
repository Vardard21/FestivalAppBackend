using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class InteractionSendDto
    {
        public string UserName { get; set; }
        public int InteractionType { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageShortDto Message { get; set; }
    }
}
