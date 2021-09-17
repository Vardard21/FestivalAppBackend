using FestivalApplication.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FestivalApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController : ControllerBase
    {
        // POST api/<UserActivityController>
        [HttpPost]
        public void Post([FromBody] UserActivity Activity)
        {
        }

        // PUT api/<UserActivityController>/5
        [HttpPut("{UserID}")]
        public void Put(int UserID, [FromBody] UserActivity Activity)
        {
        }

        // Find Stage met StageID
        // Find All UserActivity from StageID // INNER JOIN UserActivity
        // Find All Message for Users from UserActivity // INNER JOIN Messages UserActivity On UserActivityID
        // Sort Message by date, limit 50 // SortBy date, limit 50;

        // Find Stage with StageID
        // Sort Message by date, limit 50
    }
}
