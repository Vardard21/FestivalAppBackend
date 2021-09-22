using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public List<UserActivity> Log { get; set; }
    }
}
