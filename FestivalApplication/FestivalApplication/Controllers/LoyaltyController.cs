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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FestivalApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoyaltyController : ControllerBase
    {
        private readonly DBContext _context;

        public LoyaltyController(DBContext context)
        {
            _context = context;
        }
        // GET: api/<LoyaltyController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/oyaltyController>/5
        [HttpGet("{id}")]
        public Response<LoyaltyPointsDto> GetLoyaltyPoints(int id)
        {
            //creates a response variable to be sent
            Response<LoyaltyPointsDto> response = new Response<LoyaltyPointsDto>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Find user with id
                    User user = _context.User.Find(id);

                        //Create a new loyalty points DTO and fill the id, name and points
                        LoyaltyPointsDto dto = new LoyaltyPointsDto();
                        dto.UserID = user.UserID;
                        dto.UserName = user.UserName;
                        dto.Points = CalculatePoints(dto.UserID);


                    response.Success = true;
                    response.Data = dto;
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

        private int CalculatePoints(int id)
        {
            int messagepoints = _context.Message.Where(x=>x.UserActivity.User.UserID == id).Count();
            int likepoints = _context.Interaction.Where(x => x.UserActivity.User.UserID == id).Count();
            int likedpoints = _context.Interaction.Where(x => x.Message.UserActivity.User.UserID == id).Count();
            int points = messagepoints + likepoints + likedpoints;
            return points;
        }
    }
}
