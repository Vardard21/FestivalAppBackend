using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Role { get; set; }
        public List<UserActivity> Log { get; set; }
    }
}
