using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an admin
                    if (_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an admin
                        response.InvalidOperation();
                        return response;
                    }
                    //create variable to collect history of all penalties
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
                else
                {
                    response.AuthorizationError();
                    return response;
                }
            }
            catch
            {
                response.ServerError();
                return response;
            }
        }

        // GET: api/Penalty/5
        [HttpGet("{UserID}")]
        public Response<List<PenaltySendDto>> GetPenaltyHistoryID(int UserID)
        {
            //creates a response variable to be sent
            Response<List<PenaltySendDto>> response = new Response<List<PenaltySendDto>>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an admin
                    if (_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an admin
                        response.InvalidOperation();
                        return response;
                    }
                        //create variable to check history of all penalties
                        var penaltyhistory = _context.Penalty
                        .Where(x => x.UserID == UserID)
                        .ToList();

                    if (penaltyhistory.Count() == 0)
                    {
                        response.InvalidData();
                        Console.WriteLine("User does not exist");
                        return response;
                    }

                    //create a list of the penalties for all users
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
                else
                { 
                    response.AuthorizationError();
                    return response;
                }
            }
            catch
            {
                response.ServerError();
                return response;
            }
        }

        // PUT: api/Penalty/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public Response<string> PutPenalty(PenaltyChangeDto changepenalty)
        {
            //Create a new response
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an admin
                    if (_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an admin
                        response.InvalidOperation();
                        return response;
                    }

                    //Check if the penalty exists
                    if ((_context.Penalty.Any(x => x.PenaltyID == changepenalty.PenaltyID)))
                    {

                        //Change the penalty
                        Penalty penalty = _context.Penalty.Find(changepenalty.PenaltyID);
                        penalty.PenaltyType = changepenalty.PenaltyType;
                        penalty.Comment = changepenalty.Comment;
                        penalty.EndTime = changepenalty.EndTime;
                        _context.Entry(penalty).State = EntityState.Modified;

                        //Save the changes
                        if (_context.SaveChanges() > 0)
                        {
                            //Penalty was updated succesfully
                            response.Success = true;
                            return response;
                        }
                        else
                        {
                            //Error in changing penalty
                            response.ServerError();
                            response.Data = "State has not been updated";
                            return response;
                        }
                    }
                    else
                    {
                        //Penalty does not exist
                        response.Success = false;
                        response.InvalidData();
                        response.Data = "Penalty does not exist";
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
                return response;
            }
        }

        // POST: api/Penalty
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public Response<string> PostPenalty(PenaltyReceiveDto penaltyreceivedto)
        {
            //Create a new response with type string
            Response<string> response = new Response<string>();

            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    if (!_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //Check if the user exists
                        if ((_context.User.Any(x => x.UserID == penaltyreceivedto.UserID)))
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
                                //Penalty was saved correctly
                                response.Success = true;
                                response.Data = "Penalty succesfully applied";
                                return response;
                            }
                            else
                            {
                                //Penalty was not saved correctly
                                response.ServerError();
                                response.Data = "Penalty failed to apply";
                                return response;
                            }
                        }
                        else
                        {
                            //User does not exist
                            response.Success = false;
                            response.InvalidData();
                            response.Data = "User does not exist";
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
        public Response<string> DeletePenalty(int id)
        {
            //Create a new response with type string
            Response<string> response = new Response<string>();

            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    if (!_context.Authentication.Any(x => x.User.Role == "admin" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        var penalty = _context.Penalty.Find(id);
                        if (penalty == null)
                        {
                            response.InvalidData();
                            return response;
                        }

                        _context.Penalty.Remove(penalty);
                        _context.SaveChanges();

                        response.Success = true;
                        response.Data = "Penalty succesfully deleted";
                        return response;
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

        private bool PenaltyExists(int id)
        {
            return _context.Penalty.Any(e => e.PenaltyID == id);
        }
        private UserSendDto FindAssignedUser(int id)
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
