using FestivalApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class UserActivity
    {
        public int UserActivityID { get; set; }
        public User User { get; set; }
        public Stage Stage { get; set; }
        public DateTime Entry { get; set; }
        public DateTime? Exit { get; set; }
        public List<Message> MessageHistory { get; set; }

        public UserActivity()
        {

        }
    }
}
