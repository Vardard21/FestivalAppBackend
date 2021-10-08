using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class PenaltySendDto
    {
        public int PenaltyType { get; set; }
        public string Comment { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int StageID { get; set; }
        public UserSendDto Admin { get; set; }
        public UserSendDto User { get; set; }
    }
}
