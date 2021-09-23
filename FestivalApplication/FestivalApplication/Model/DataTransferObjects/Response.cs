using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model.DataTransferObjects
{
    public class Response<o>
    {
        public Boolean Success { get; set; }
        public List<int> ErrorMessage { get; set; } = new List<int>();
        public o Data { get; set; }
    }
}
