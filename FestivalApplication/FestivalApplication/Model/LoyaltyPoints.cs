using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class LoyaltyPoints
    {
        public int ID { get; set; }
        public User User { get; set; }
        public int Points { get; set; }
    }
}
