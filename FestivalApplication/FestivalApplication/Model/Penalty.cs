using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class Penalty
    {
        public int PenaltyID { get; set; }
        public int UserID { get; set; }
        public int PenaltyType { get; set; }
        public int StageID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AdminID { get; set; }
        public string Comment { get; set; }

    }
}
