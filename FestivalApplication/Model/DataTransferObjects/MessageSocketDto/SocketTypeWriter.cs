using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class SocketTypeWriter<o>
    {
        public string MessageType { get; set; }
        public o Message { get; set; }
    }
}
