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

            //create a stages variable to be checked
            var stagesstatus = _context.Stage
                .Where(x => x.StageActive == true)
                .ToList();


            //create a list of active stages
            List<StagesRequestDto> ActiveStages = new List<StagesRequestDto>();

            if (!ActiveStages.Any())
            {

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
            else
            {
                response.Success = false;
                response.Data = ActiveStages;
                response.ErrorMessage.Add(1);
                return response;
            }
        }

        // POST: api/Message
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public Response<string> PostStage(StageCreateDto stagecreatedto)
        {
            //creates a response variable to be sent
            Response<string> response = new Response<string>();

            //create a new stage to be inserted in DB
            Stage newStage = new Stage();

            //check if the new stage has name of an existing stage
            var stagefound = _context.Stage.Where(x => x.StageName == stagecreatedto.StageName).ToList();


            if (stagefound.Count() == 1)
            {

                response.Success = false;
                response.Data = "Stage with the same name already exists";
                response.ErrorMessage.Add(3);
                return response;

            }
            else
            {
                newStage.StageName = stagecreatedto.StageName;
                newStage.StageActive = stagecreatedto.StageActive;
                _context.Stage.Add(newStage);

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
                    response.Data = "Error while saving stage";
                    response.ErrorMessage.Add(1);
                    return response;
                }
            }
        }
        // PUT: api/StagesController2/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public Response<string> PutUser(StageUpdateDto changestage)
        {
            //Create a new response
            Response<string> response = new Response<string>();
            try
            {
                //Check if the stageID exists
                if ((_context.Stage.Any(x => x.StageID == changestage.StageID)))
                    
                {
                    //Check if the state is actually different
                    if (!_context.Stage.Any(x => x.StageActive == changestage.StageActive))
                    {
                        //Stage is already at that state
                        response.Success = false;
                        response.ErrorMessage.Add(2);
                        response.Data = "Stage is already in that state";
                        return response;
                    }
                    //Change the state
                    Stage stage = _context.Stage.Find(changestage.StageID);
                    stage.StageActive = changestage.StageActive;
                    _context.Entry(stage).State = EntityState.Modified;

                    //Save the changes
                    if (_context.SaveChanges() > 0)
                    {
                        //Stage was updated succesfully
                        response.Success = true;
                        return response;
                    }
                    else
                    {
                        //stage was not updated succesfully
                        response.Success = false;
                        response.ErrorMessage.Add(1);
                        response.Data = "State has not been updated";
                        return response;
                    }
                }
                else
                {
                    //Stage does not exist
                    response.Success = false;
                    response.ErrorMessage.Add(2);
                    response.Data = "Stage does not exist";
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

