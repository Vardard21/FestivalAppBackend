using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class UserActivity
    {
        public int UserActivityID { get; set; }
        public int UserID { get; set; }
        public int StageID { get; set; }
        public DateTime Entry { get; set; }
        public DateTime Exit { get; set; }
        public List<Message> MessageHistory { get; set; }
    }
}
