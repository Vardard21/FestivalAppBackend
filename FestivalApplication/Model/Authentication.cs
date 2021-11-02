using FestivalApplication.Data;
using FestivalApplication.Model.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalApplication.Model
{
    public class Authentication
    {
        public int AuthenticationID { get; set; }
        public User User { get; set; }
        public string AuthenticationKey { get; set; }
        public DateTime MaxExpiryDate { get; set; }
        public DateTime CurrentExpiryDate { get; set; }
    }
}
