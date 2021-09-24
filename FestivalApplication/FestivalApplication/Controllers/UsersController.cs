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
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DBContext _context;

        public UsersController(DBContext context)
        {
            _context = context;
        }

        // GET an overview of all users
        [HttpGet]
        public Response<List<UserSendDto>> GetUser()
        {
            //Create a new response
            Response<List<UserSendDto>> response = new Response<List<UserSendDto>>();
            try
            {
                //Get a list of all users
                var AllUsers = _context.User.ToList();

                //Convert the list to dto objects
                List<UserSendDto> sendlist = new List<UserSendDto>();
                foreach (User user in AllUsers)
                {
                    UserSendDto dto = new UserSendDto();
                    dto.UserID = user.UserID;
                    dto.UserName = user.UserName;
                    dto.UserRole = user.Role;
                    sendlist.Add(dto);
                }

                //return the list
                response.Success = true;
                response.Data = sendlist;
                return response;
            }
            catch
            {
                response.Success = false;
                response.ErrorMessage.Add(1);
                return response;
            }

        }

        // GET specific user data
        [HttpGet("{id}")]
        public Response<UserDetailsDto> GetUser(int id)
        {
            //Create a new response
            Response<UserDetailsDto> response = new Response<UserDetailsDto>();
            try
            {
                //Validate that the user exists
                if (!_context.User.Any(x => x.UserID == id))
                {
                    //User does not exist
                    response.Success = false;
                    response.ErrorMessage.Add(2);
                    return response;
                }

                //Get the user and the user details
                User user = _context.User.Find(id);
                UserDetailsDto dto = new UserDetailsDto();
                dto.UserID = user.UserID;
                dto.UserName = user.UserName;
                dto.UserRole = user.Role;
                List<UserActivity> UserActivities = _context.UserActivity.Where(x => x.UserID == id).ToList();
                dto.activities = new List<UserActivityWithMessageDto>();
                foreach(UserActivity activity in UserActivities)
                {
                    UserActivityWithMessageDto uadto = new UserActivityWithMessageDto();
                    uadto.StageID = activity.StageID;
                    uadto.Entry = activity.Entry;
                    uadto.Exit = activity.Exit;
                    List<Message> messages = _context.Message.Where(x => x.UserActivity.UserActivityID == activity.UserActivityID).ToList();
                    uadto.MessageHistory = new List<MessageShortDto>();
                    foreach(Message message in messages)
                    {
                        MessageShortDto mdto = new MessageShortDto();
                        mdto.MessageText = message.MessageText;
                        mdto.Timestamp = message.Timestamp;
                        uadto.MessageHistory.Add(mdto);
                    }
                    dto.activities.Add(uadto);
                }
                response.Success = true;
                response.Data = dto;
                return response;
            }
            catch
            {
                response.Success = false;
                response.ErrorMessage.Add(1);
                return response;
            }
        }

        // Update User Data (Role)
        [HttpPut]
        public Response<string> PutUser(ChangeRoleDto changerequest)
        {
            //Create a new response
            Response<string> response = new Response<string>();
            try
            {
                //Validate the role
                if(changerequest.UserRole == "admin" | changerequest.UserRole == "artist" | changerequest.UserRole == "visitor")
                {
                    //Validate the user
                    if (!_context.User.Any(x => x.UserID == changerequest.UserID))
                    {
                        //User does not exist
                        response.Success = false;
                        response.ErrorMessage.Add(2);
                        return response;
                    }
                    //Validate that the role is actually new
                    if (_context.User.Any(x => x.UserID == changerequest.UserID && x.Role == changerequest.UserRole))
                    {
                        //User already has this role
                        response.Success = false;
                        response.ErrorMessage.Add(2);
                        return response;
                    }
                    //Change the role
                    User user = _context.User.Find(changerequest.UserID);
                    user.Role = changerequest.UserRole;
                    _context.Entry(user).State = EntityState.Modified;

                    //Save the changes
                    if (_context.SaveChanges() > 0)
                    {
                        //Message was saved correctly
                        response.Success = true;
                        return response;
                    }
                    else
                    {
                        //Message was not saved correctly
                        response.Success = false;
                        response.ErrorMessage.Add(1);
                        return response;
                    }
                }
                else
                {
                    //Role does not exist
                    response.Success = false;
                    response.ErrorMessage.Add(2);
                    return response;
                }
            }
            catch
            {
                response.Success = false;
                response.ErrorMessage.Add(1);
                return response;
            }
        }

        //Create new users
        [HttpPost]
        public Response<int> PostUser(UserCreationDto user)
        {
            //Create a new response
            Response<int> response = new Response<int>();
            try { 
                //Validate that the fields exist
                if (user.PassWord != null && user.UserName != null)
                {
                    //Validate thatthe username does not exist
                    if (_context.User.Any(x => x.UserName == user.UserName))
                    {
                        response.Success = false;
                        response.ErrorMessage.Add(2);
                        return response;
                    }
                    User NewUser = new User();
                    NewUser.UserName = user.UserName;
                    NewUser.PassWord = user.PassWord;
                    NewUser.Role = "visitor";
                    _context.User.Add(NewUser);
                }

                //Save the changes
                if (_context.SaveChanges() > 0)
                {
                    //Message was saved correctly
                    response.Success = true;
                    response.Data = _context.User.Where(x => x.UserName == user.UserName).First().UserID;
                    return response;
                }
                else
                {
                    //Message was not saved correctly
                    response.Success = false;
                    response.ErrorMessage.Add(1);
                    return response;
                }
            }
            catch
            {
                response.Success = false;
                response.ErrorMessage.Add(1);
                return response;
            }
        }
    }
}
