using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class LoyaltyPointsDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int Points { get; set; }
    }
}
