using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PenaltyReceiveDto
    {
        public int PenaltyType { get; set; }
        public int UserID { get; set; }
        public int StageID { get; set; }
        public int AdminID { get; set; }
        public string Comment { get; set; }
    }
}
