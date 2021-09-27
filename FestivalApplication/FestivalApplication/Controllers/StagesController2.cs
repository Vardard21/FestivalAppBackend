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
    public class StagesController2 : ControllerBase
    {
        private readonly DBContext _context;

        public StagesController2(DBContext context)
        {
            _context = context;
        }

        // GET: api/<StageController2>
        [HttpGet("id")]
        public Response<List<StageUsersDto>> GetStageUsers(int id)
        {
            //creates a response variable to be sent
            Response<List<StageUsersDto>> response = new Response<List<StageUsersDto>>();

            //checks if stage exists
            if(!_context.Stage.Any(x=>x.StageID==id))
            {
                //Stage does not exist
                response.Success = false;
                response.ErrorMessage.Add(2);
                return response;
            }

            //create a stages variable to be checked
            var stageusers = _context.UserActivity
                .Where(x => x.StageID == id && x.Exit==default)
                .ToList();

            List<StageUsersDto> ActiveUsers = new List<StageUsersDto>();

            if (!ActiveUsers.Any())
            {

                //create a for loop for each stage in stage status
                foreach (UserActivity useractivity in stageusers)
                {
                    //Create a new Stage Request DTO and fill the id and name
                    StageUsersDto dto = new StageUsersDto();
                    User user = _context.User.Find(useractivity.UserID);
                    dto.UserID = user.UserID;
                    dto.UserName = user.UserName;
                    dto.UserRole = user.Role;


                    //Add the new object to the return list
                    ActiveUsers.Add(dto);
                }

                response.Success = true;
                response.Data = ActiveUsers;
                return response;
            }
            else
            {
                response.Success = false;
                response.Data = ActiveUsers;
                response.ErrorMessage.Add(1);
                return response;
            }
        }

       
    }
}
