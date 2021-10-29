using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class MessageShortDto
    {
        public int MessageID { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
