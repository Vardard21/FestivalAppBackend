using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class Interaction
    {
        public int InteractionID { get; set; }
        public int InteractionType { get; set; }
        public DateTime Timestamp { get; set; }
        public UserActivity UserActivity { get; set; }
        public Message Message { get; set; }
    }
}
