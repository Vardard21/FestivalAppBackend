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

            if(!ActiveStages.Any())
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


            if (stagefound.Count()==1 )
            {

                response.Success = false;
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
                    response.ErrorMessage.Add(1);
                    return response;
                }
            }
        }
        [HttpPut("{id}")]
        public Response<string> UpdateStage(int id, StageUpdateDto stageUpdatedto)
        {

                //creates a response variable to be sent
                Response<string> response = new Response<string>();


                    if (_context.Stage.Where(x => x.StageID == stageUpdatedto.StageID).ToList()==default)
                    {
                        response.Success = false;
                        response.ErrorMessage.Add(2);
                        return response;
                    }
                  

                    //create a new stage to be inserted in DB
                    Stage newStage = new Stage();

                    //check if the new stage has name of an existing stage
                        newStage.StageActive = stageUpdatedto.StageActive;
                         _context.Stage.Append(newStage);

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
                                response.Data= "Failed to save Data" ;
                                return response;

                            }
                    }
                
    }
}

