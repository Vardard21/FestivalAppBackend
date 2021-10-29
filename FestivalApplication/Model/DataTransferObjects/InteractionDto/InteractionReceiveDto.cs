using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class InteractionReceiveDto
    {
        public int MessageID { get; set; }
        public int UserID { get; set; }
        public int InteractionType { get; set; }
    }
}
