using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class MessageInteractionsDto
    {
        public int MessageID { get; set; }
        public List<InteractionDto> Interactions { get; set; }
    }
}
