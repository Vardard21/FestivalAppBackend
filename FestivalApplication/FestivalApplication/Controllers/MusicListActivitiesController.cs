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
    public class MusicListActivitiesController : ControllerBase
    {
        private readonly DBContext _context;

        public MusicListActivitiesController(DBContext context)
        {
            _context = context;
        }

        // GET: api/MusicListActivities
        [HttpGet]
        public Response<List<Track>> GetMusicListActivity()
        {
            Response<List<Track>> response = new Response<List<Track>>();
            return response;
        }

        // GET: api/MusicListActivities/5
        [HttpGet("{id}")]
        public Response<List<PlaylistRequestDto>> GetMusicListActivity(int id)
        {
            Response<List<PlaylistRequestDto>> response = new Response<List<PlaylistRequestDto>>();
            

            if (!(_context.MusicList.Where(x=>x.ID==id).Count()==1))
            {
                response.InvalidData();
                return response;
            }

            //find the list in list activities
            var playlist = _context.TrackActivity.Where(x => x.MusicListID == id&& x.Playing==true).ToList();

            //create a list of tracks
            List<PlaylistRequestDto> RequestedTracks = new List<PlaylistRequestDto>();

            if (!RequestedTracks.Any())
            {

                foreach (TrackActivity trackactivity in playlist)
                {

                    Track track = _context.Track.Find(trackactivity.TrackID);
                    PlaylistRequestDto dto = new PlaylistRequestDto();
                    dto.Id = trackactivity.TrackID;
                    dto.TrackName = track.TrackName;
                    dto.TrackSource = track.TrackSource;
                    dto.Length = track.Length;
                    RequestedTracks.Add(dto);
                }
                response.Success = true;
                response.Data = RequestedTracks;
                return response;
            }
            else
            {
                response.ServerError();
                return response;
            }
        }

        // PUT: api/MusicListActivities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("p/{id}")]
        public Response<PlaylistSendDto> UpdateMusicActivity(int id, int musicid)
        {
            //create a response to send back
            Response<PlaylistSendDto> response = new Response<PlaylistSendDto>();
            try
            {
                //checks user authentification
                AuthenticateKey auth = new AuthenticateKey();
                if (!auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //check if user is actually an artist ||REVERSED ATM||
                    if (_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    {
                        //User is not an artist
                        response.InvalidOperation();
                        Console.WriteLine("User is not an artist");
                        return response;
                    }

                    // check if the playlist exists
                    if (!(_context.MusicList.Where(x => x.ID == musicid).Count() == 1))
                    {
                        response.InvalidData();
                        Console.WriteLine("Playlist doesnt exist");
                        return response;
                    }

                    //checks which tracks are in the musiclist
                    var playlist = _context.TrackActivity.Where(x => x.MusicListID == musicid).ToList();

                    //turn all playing to false
                    foreach (TrackActivity trackactivity in playlist)
                    {
                        {
                            trackactivity.Playing = false;
                            _context.Entry(trackactivity).State = EntityState.Modified;
                        }
                    }

                    //find selected track
                    var selectedtrack = _context.TrackActivity.Where(x=>x.TrackID==id).ToList();

                    //check if selected track only has 1 entry
                    if (selectedtrack.Count() == 1)
                    {
                        foreach (TrackActivity trackactivity in selectedtrack)
                        {
                            {
                                trackactivity.Playing = true;
                                Track track =_context.Track.Find(trackactivity.TrackID);
                                PlaylistSendDto dto = new PlaylistSendDto();
                                dto.TrackName = track.TrackName;


                                _context.Entry(trackactivity).State = EntityState.Modified;
                                if (_context.SaveChanges() > 0)
                                {
                                    //Track has been set to playing
                                    response.Success = true;
                                    response.Data=dto ;
                                    return response;
                                }
                                else
                                {
                                    //Error in saving track
                                    response.ServerError();
                                    return response;
                                }
                            }
                        }
                    }
                    else
                    {
                        response.InvalidData();

                        return response;
                    }

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
    }
}
