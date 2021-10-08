using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;


namespace FestivalApplication.Controllers
{
    [Route("api/Penalty")]
    [ApiController]
    public class PenaltyController : ControllerBase
    {
        private readonly DBContext _context;

        public PenaltyController(DBContext context)
        {
            _context = context;
        }

        // GET: api/Penalty
        [HttpGet]
        public Response<List<PenaltySendDto>> GetPenaltyHistory()
        {   
            //creates a response variable to be sent
            Response<List<PenaltySendDto>> response = new Response<List<PenaltySendDto>>();
            {
                //create variable to check expiry of penalty
                var penaltyhistory = _context.Penalty
                .ToList();

                

                //create a list of the active penalties for all users
                List<PenaltySendDto> PenaltyHistory = new List<PenaltySendDto>();

                //only proceed when there are active penalties
                if (!PenaltyHistory.Any())
                {
                    //create a for loop for each penalty in penalty status
                    foreach (Penalty penalty in penaltyhistory)
                    {
                        //Create a new Penalty Send DTO 
                        PenaltySendDto dto = new PenaltySendDto();
                        //fill the requested attributes
                        dto.PenaltyType = penalty.PenaltyType;
                        dto.Comment = penalty.Comment;
                        dto.StartTime = penalty.StartTime;
                        dto.EndTime = penalty.EndTime;
                        //The bannedstage is not always used, so create if else statement to
                        //give null when the penalty does not specify a stage
                        if (penalty.StageID!=default)
                        {
                            dto.StageID = penalty.StageID;
                        }
                        else
                        {
                            dto.StageID = default;
                        }
                        //User info is connected through classes

                        dto.Admin = FindAssignedUser(penalty.AdminID);
                        dto.User = FindAssignedUser(penalty.UserID);

                        //Add the new object to the return list
                        PenaltyHistory.Add(dto);
                    }

                }

                response.Success = true;
                response.Data = PenaltyHistory;
                return response;
            }
        }

        // GET: api/Penalty/5
        [HttpGet("{id}")]
        public Response<List<PenaltySendDto>> GetPenaltyHistoryID(int id)
        {
            //creates a response variable to be sent
            Response<List<PenaltySendDto>> response = new Response<List<PenaltySendDto>>();
            {
                //create variable to check expiry of penalty
                var penaltyhistory = _context.Penalty
                .Where(x=> x.UserID==id)
                .ToList();



                //create a list of the active penalties for all users
                List<PenaltySendDto> PenaltyHistory = new List<PenaltySendDto>();

                //only proceed when there are active penalties
                if (!PenaltyHistory.Any())
                {
                    //create a for loop for each penalty in penalty status
                    foreach (Penalty penalty in penaltyhistory)
                    {
                        //Create a new Penalty Send DTO 
                        PenaltySendDto dto = new PenaltySendDto();
                        //fill the requested attributes
                        dto.PenaltyType = penalty.PenaltyType;
                        dto.Comment = penalty.Comment;
                        dto.StartTime = penalty.StartTime;
                        dto.EndTime = penalty.EndTime;
                        //The bannedstage is not always used, so create if else statement to
                        //give null when the penalty does not specify a stage
                        if (penalty.StageID != default)
                        {
                            dto.StageID = penalty.StageID;
                        }
                        else
                        {
                            dto.StageID = default;
                        }
                        //User info is connected through classes

                        dto.Admin = FindAssignedUser(penalty.AdminID);
                        dto.User = FindAssignedUser(penalty.UserID);

                        //Add the new object to the return list
                        PenaltyHistory.Add(dto);
                    }

                }

                response.Success = true;
                response.Data = PenaltyHistory;
                return response;
            }
        }

        // PUT: api/Penalty/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPenalty(int id, Penalty penalty)
        {
            if (id != penalty.PenaltyID)
            {
                return BadRequest();
            }

            _context.Entry(penalty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PenaltyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Penalty
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public Response<string> PostPenalty(PenaltyReceiveDto penaltyreceivedto)
        {
            //Create a new response with type string
            Response<string> response = new Response<string>();

            try {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    if (!_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {

                        //creates a response variable to be sent
                        Penalty newPenalty = new Penalty();
                        newPenalty.PenaltyType = penaltyreceivedto.PenaltyType;
                        newPenalty.AdminID = penaltyreceivedto.AdminID;
                        newPenalty.StageID = penaltyreceivedto.StageID;
                        newPenalty.UserID = penaltyreceivedto.UserID;
                        newPenalty.Comment = penaltyreceivedto.Comment;
                        newPenalty.StartTime = DateTime.UtcNow;
                        newPenalty.EndTime = DateTime.UtcNow.AddMinutes(15); ;
                        _context.Penalty.Add(newPenalty);
                        if (_context.SaveChanges() > 0)
                        {
                            //Message was saved correctly
                            response.Success = true;
                            response.Data = "Penalty succesfully applied";
                            return response;
                        }
                        else
                        {
                            //Message was not saved correctly
                            response.ServerError();
                            response.Data = "Penalty failed to apply";
                            return response;
                        }
                    }
                    else
                    {
                        response.InvalidOperation();
                        response.Data = "User is not an admin";
                        return response;
                    }
                }
                else
                {
                    response.AuthorizationError();
                    return response;
                }
            }
            catch
            {
                response.ServerError();
                response.Data = "Failed to connect to database";
                return response;
            }
        }

        // DELETE: api/Penalty/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePenalty(int id)
        {
            var penalty = await _context.Penalty.FindAsync(id);
            if (penalty == null)
            {
                return NotFound();
            }

            _context.Penalty.Remove(penalty);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PenaltyExists(int id)
        {
            return _context.Penalty.Any(e => e.PenaltyID == id);
        }
        public UserSendDto FindAssignedUser(int id)
        {
            //create user send dto
            UserSendDto dto = new UserSendDto();
            //Find the admins from user class
            User user = _context.User.Find(id);
            //select desired attributes of admin users
            dto.UserID = user.UserID;
            dto.UserName = user.UserName;
            dto.UserRole = user.Role;
            return dto;

        }
    }
}
