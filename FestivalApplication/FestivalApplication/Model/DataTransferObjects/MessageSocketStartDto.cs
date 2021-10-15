using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class MessageSocketStartDto
    {
        public string AuthenticationKey { get; set; }
        public int StageID { get; set; }
    }
}
