using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PenaltyChangeDto
    {
        public int PenaltyID { get; set; }
        public int PenaltyType { get; set; }
        public string Comment { get; set; }
        public DateTime EndTime { get; set; }
    }
}
