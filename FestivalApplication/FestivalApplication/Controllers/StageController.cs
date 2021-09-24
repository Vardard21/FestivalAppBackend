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
    public class StageController : ControllerBase
    {
        private readonly DBContext _context;

        public StageController(DBContext context)
        {
            _context = context;
        }
        // GET: api/<StageController>
        [HttpGet]
        public Response<List<StagesRequestDto>> GetStages()
        {
            //creates a response variable to be sent
            Response<List<StagesRequestDto>> response = new Response<List<StagesRequestDto>>();

            //create a stages variable to be sent
            var stagesstatus = _context.Stage
                .Where(x => x.StageActive == true)
                .ToList();

            //create a list of active stages
            List<StagesRequestDto> ActiveStages = new List<StagesRequestDto>();

            //create a for loop for each stage in stage status
            foreach (Stage stage in stagesstatus)
            {
                //Create a new Stage Request DTO and fill the id and name
                StagesRequestDto dto = new StagesRequestDto();
                dto.StageID = stage.StageID;
                dto.StageName = stage.StageName;
                //Find the Current Song, temporarily only manually defined
                dto.CurrentSong = "Thunderstruck by AC/DC";
                ////Find the amount of active users in the stage
                dto.NumberOfUsers = _context.UserActivity
                    .Where(x => x.StageID == stage.StageID)
                    .Where(x => x.Exit == default)
                    .Count();
                //Add the new object to the return list
                ActiveStages.Add(dto);
            }

            response.Success = true;
            response.Data = ActiveStages;
            return response;
        }

        
    }
}
