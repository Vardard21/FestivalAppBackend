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
    [Route("api/Track")]
    [ApiController]
    public class TrackController : ControllerBase
    {

        private readonly DBContext _context;

        public TrackController(DBContext context)
        {
            _context = context;
        }
        // GET: api/<TrackController>
        [HttpGet]
        public Response<List<TrackRequestDto>> GetTracks()
        {
            Response<List<TrackRequestDto>> response = new Response<List<TrackRequestDto>>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an artist
                        response.InvalidOperation();
                        return response;
                    }


                    //Get a list of all music list
                    var existingTracks = _context.Track.ToList();
                    //Convert the list to dto objects
                    List<TrackRequestDto> tracklist = new List<TrackRequestDto>();

                    //create a for loop for each stage in stage status
                    foreach (Track track in existingTracks)
                    {
                        //Create a new Stage Request DTO and fill the id and name
                        TrackRequestDto dto = new TrackRequestDto();
                        dto.TrackID = track.TrackID;
                        dto.TrackName = track.TrackName;
                        dto.TrackSource = track.TrackSource;

                        tracklist.Add(dto);
                    }

                    response.Success = true;
                    response.Data = tracklist;
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

        // GET api/<TrackController>/5
        [HttpGet("{id}")]
        public Response<List<TrackItemDto>> GetTrack(int id)
        {
            Response<List<TrackItemDto>> response = new Response<List<TrackItemDto>>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //checks if track exists
                    if (!_context.Track.Any(x => x.TrackID == id))
                    {
                        //Stage does not exist
                        response.InvalidData();
                        return response;
                    }
                    if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an artist
                       response.InvalidOperation();
                        return response;
                    }

                    //create a track variable to be checked
                    var trackitem = _context.Track
                        .Where(x => x.TrackID == id)
                        .ToList();


                    List<TrackItemDto> trackdetails = new List<TrackItemDto>();


                    foreach (Track track in trackitem)
                    {

                        TrackItemDto dto = new TrackItemDto();
                        Track user = _context.Track.Find(track.TrackID);
                        dto.TrackID = track.TrackID;
                        dto.TrackName = track.TrackName;
                        dto.TrackSource = track.TrackSource;


                        //Add the new object to the return list
                        trackdetails.Add(dto);
                    }

                    response.Success = true;
                    response.Data = trackdetails;
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

        // POST api/<TrackController>
        [HttpPost]
         public Response<string> PostTrack(TrackNewDto tracknewdto){
            //creates a response variable to be sent
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an artist
                    if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an artist
                        response.InvalidOperation();
                        return response;
                    }

                    //create a new track to be inserted in DB
                    Track newTrack = new Track();

                    //check if the new track has name of an existing track
                    var trackfound = _context.Track.Where(x => x.TrackSource == tracknewdto.TrackSource).ToList();

                    


                    if (trackfound.Count() == 1)
                    {

                        response.InvalidOperation();
                        response.Data = "Track with the same source already exists";
                        return response;

                    }
                    else if(tracknewdto.TrackSource == "" ){
                        response.InvalidOperation();
                        response.Data = "Track has no source";
                        return response;
                    }
                    else if (tracknewdto.TrackName == "" )
                    {
                        response.InvalidOperation();
                        response.Data = "Track has no name";
                        return response;
                    }
                    else
                    {
                        newTrack.TrackName = tracknewdto.TrackName;
                        newTrack.TrackSource = tracknewdto.TrackSource;
                        
                        _context.Track.Add(newTrack);

                        if (_context.SaveChanges() > 0)
                        {
                            //Message was saved correctly
                            response.Success = true;
                            return response;
                        }
                        else
                        {
                            //Message was not saved correctly
                            response.ServerError();
                            response.Data = "Error while saving track";
                            return response;
                        }
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



        // PUT api/<TrackController>/5
        [HttpPut]
        public Response<string> PutTrack(TrackUpdateDto changetrack)
        {
            //Create a new response
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an admin
                    if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User changing the role is not an admin
                        response.InvalidOperation();
                       return response;
                   }

                    //Check if the stageID exists
                    if ((_context.Track.Any(x => x.TrackID == changetrack.TrackID)))

                    {
                        //Check if the source is actually different
                        if (_context.Track.Any(x => x.TrackID == changetrack.TrackID && x.TrackSource == changetrack.TrackSource))
                        {
                            //Source is not different
                            response.InvalidData();
                            response.Data = "Source is the same";
                            return response;
                        }
                        //Change the source
                       Track track = _context.Track.Find(changetrack.TrackID);
                        track.TrackSource = changetrack.TrackSource;
                        _context.Entry(track).State = EntityState.Modified;

                        //Save the changes
                        if (_context.SaveChanges() > 0)
                        {
                            //Stage was updated succesfully
                            response.Success = true;
                            return response;
                        }
                        else
                        {
                            //Error in updating track
                            response.ServerError();
                            response.Data = "Track has not been updated";
                            return response;
                        }
                    }
                    else
                    {
                        //Track does not exist
                        response.Success = false;
                        response.InvalidData();
                        response.Data = "Track does not exist";
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

        // DELETE api/<TrackController>/5
    
        [HttpDelete("{id}")]
        public Response<string> Delete(int id)
        {
            Response<string> response = new Response<string>();
            try
            {
                //Validate the authentication key
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the user deleting the track is an artist
                    if (!_context.Authentication.Any(x => x.AuthenticationKey == Request.Headers["Authorization"] && x.User.Role == "artist"))
                    {
                        response.InvalidOperation();
                        return response;
                    }
                    //Validate that the track exist
                    Track track = _context.Track.Find(id);
                

                
                    if (track == null)
                    {
                        response.InvalidData();
                        return response;
                    }

                    //Validate that the track is not playing
                    //if (track.)
                    //{
                    //  response.InvalidOperation();
                    //return response;
                    //}
                    var activityList = _context.TrackActivity.Where(x => x.TrackID == id).ToList();
                    //Remove any interactions
                    foreach (TrackActivity activity in activityList)
                    {
                        _context.TrackActivity.Remove(activity);

                    }
                    _context.Track.RemoveRange(_context.Track.Where(x => x.TrackID == track.TrackID));
                



                    //Delete the track
                    if (_context.SaveChanges() > 0)
                    {
                        response.Success = true;
                        return response;
                    }
                    else
                    {
                        response.ServerError();
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

    }
}
