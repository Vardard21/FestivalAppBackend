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

        [HttpGet]
        public Response<List<LoyaltyPointsDto>> GetAllLoyaltyPoints()
        {
            //creates a response variable to be sent
            Response<List<LoyaltyPointsDto>> response = new Response<List<LoyaltyPointsDto>>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {


                    //Find loyaltypoints row assigned to user
                    var loyaltyPoints = _context.LoyaltyPoints.Include(y => y.User).ToList();

                    //Create a new loyalty points DTO and fill the id, name and points
                    List<LoyaltyPointsDto> allPoints = new List<LoyaltyPointsDto>();
                    foreach (LoyaltyPoints entry in loyaltyPoints)
                    {
                        LoyaltyPointsDto dto = new LoyaltyPointsDto();
                        dto.Points = entry.Points;
                        allPoints.Add(dto);

                    }

                    response.Success = true;
                    response.Data = allPoints;
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
        // PUT api/oyaltyController>/5
        [HttpPut("{id}")]
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
                    User loyaltyUser = _context.User.Find(id);

                    //Find loyaltypoints row assigned to user

                    DateTime lastupdated = default;

                    //Create a new loyalty points DTO and fill the id, name and points
                    LoyaltyPointsDto dto = new LoyaltyPointsDto();
                    dto.UserID = loyaltyUser.UserID;
                    dto.UserName = loyaltyUser.UserName;
                    
                    dto.Points = CalculatePoints(loyaltyUser, lastupdated);
                    if (_context.LoyaltyPoints.Where(x => x.ID == id).FirstOrDefault() == default)
                    {
                        LoyaltyPoints loyaltyPoints = new LoyaltyPoints();
                        loyaltyPoints.Points = dto.Points;
                        loyaltyPoints.User = loyaltyUser;
                        loyaltyPoints.LastUpdated = DateTime.UtcNow;
                        _context.LoyaltyPoints.Add(loyaltyPoints);

                    }
                    else
                    {
                        LoyaltyPoints loyaltyPoints = _context.LoyaltyPoints.Where(x => x.ID == id).First();
                        loyaltyPoints.Points = dto.Points;
                        loyaltyPoints.LastUpdated = DateTime.UtcNow;
                        _context.Entry(loyaltyPoints).State = EntityState.Modified;
                    }

                    //Save the changes
                    if (_context.SaveChanges() > 0)
                    {
                        //Entry was updated succesfully
                        response.Success = true;
                        response.Data = dto;
                        return response;
                    }
                    else
                    {
                        //Error in updating entry
                        response.ServerError();
                        response.Data.UserName = "State has not been updated";
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

        private int CalculatePoints(User user, DateTime lastupdated)
        {
            int messagepoints = 0;
            int likepoints = 0;
            int likedpoints = 0;
            int currentpoints = 0;
            int timepoints = 0;
            double timepointsdouble = 0;
            //Check Last Update
            if (lastupdated == default)
            {
                //Sum up points
                messagepoints = _context.Message.Where(x=>x.UserActivity.User.UserID == user.UserID).Count();
                likepoints = _context.Interaction.Where(x => x.UserActivity.User.UserID == user.UserID).Count();
                likedpoints = _context.Interaction.Where(x => x.Message.UserActivity.User.UserID == user.UserID).Count();
                var useractivities=_context.UserActivity.Where(x=>x.User.UserID==user.UserID&&x.Exit!=default).ToList() ;
                foreach (UserActivity activity in useractivities)
                {
                    DateTime starttime =activity.Entry;
                    DateTime endtime =activity.Exit??DateTime.Now;
                    var timediff = (endtime - starttime).TotalSeconds;
                    double calculatedtimepoints = timediff / 3600;
 
                    timepointsdouble = timepointsdouble + calculatedtimepoints;
                }
                timepoints = Convert.ToInt32(timepointsdouble);

            }
            else
            {
                //sum points since last update
                messagepoints = _context.Message.Where(x => x.UserActivity.User.UserID == user.UserID && x.UserActivity.Exit > lastupdated).Count();
                likepoints = _context.Interaction.Where(x => x.UserActivity.User.UserID == user.UserID && x.UserActivity.Exit > lastupdated).Count();
                likedpoints = _context.Interaction.Where(x => x.Message.UserActivity.User.UserID == user.UserID && x.UserActivity.Exit > lastupdated).Count();
                currentpoints = _context.LoyaltyPoints.Find(user).Points;
                timepoints = 0;
                var useractivities = _context.UserActivity.Where(x => x.User.UserID == user.UserID && x.Exit != default).ToList();
                foreach (UserActivity activity in useractivities)
                {
                    DateTime starttime = activity.Entry;
                    DateTime endtime = activity.Exit ?? DateTime.Now;
                    var timediff = (endtime - starttime).TotalSeconds;
                    double calculatedtimepoints = timediff / 3600;

                    timepointsdouble = timepointsdouble + calculatedtimepoints;
                }
                timepoints = Convert.ToInt32(timepointsdouble);
            }           
            int points = currentpoints + messagepoints + likepoints + likedpoints+timepoints;
            return points;

        }
    }
}
