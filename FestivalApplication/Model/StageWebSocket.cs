using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class StageWebSocket
    {
        public WebSocket webSocket { get; set; }
        public Stage stage { get; set; }
    }
}
