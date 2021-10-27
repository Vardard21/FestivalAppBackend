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
    [Route("api/Playlist")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly DBContext _context;

        public PlaylistController(DBContext context)
        {
            _context = context;
        }

        // GET: api/<PlaylistController>
        [HttpGet]
        public Response<List<MusicListInfoDto>> GetPlaylists()
        {
            Response<List<MusicListInfoDto>> response = new Response<List<MusicListInfoDto>>();

            List<MusicListInfoDto> Tracks = new List<MusicListInfoDto>();
            try
            {

                 AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"])) { 

                    //create a list of tracks to add to each musiclist
                    var MusicLists = _context.MusicList.Include(y=>y.MusicTracks).ToList();

                    //if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    //{
                    //    //User changing the role is not an artist
                    //    response.InvalidOperation();
                    //    return response;
                    //}

                    foreach (MusicList musiclist in MusicLists)
                {
                    //find the list in list activities
                    var playlist = _context.TrackActivity.Where(x => x.MusicListID == musiclist.ID).ToList();

                    //create a list of tracks
                    List<PlaylistRequestDto> PlaylistTracks = new List<PlaylistRequestDto>();

                    //define the playlist
                    MusicListInfoDto listdto = new MusicListInfoDto();
                    listdto.ID = musiclist.ID;
                    listdto.Name = musiclist.ListName;
                      
                    foreach (TrackActivity trackactivity in playlist)
                    {
                          
                        //add the track to the playlist
                        Track track = _context.Track.Where(x=> x.TrackID==trackactivity.TrackID).First();
                        PlaylistRequestDto dto = new PlaylistRequestDto();
                        dto.Id = track.TrackID;
                        dto.TrackName = track.TrackName;
                        dto.TrackSource = track.TrackSource;
                        PlaylistTracks.Add(dto);
                    }
                    //add the playlist to the list of playlists
                    listdto.PlaylistTracks = PlaylistTracks;
                    Tracks.Add(listdto);
                }
                //add the list of playlists to the response and send it back
                response.Success = true;
                response.Data = Tracks;
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
                //server error
                response.ServerError();
                return response;
            }

        }

        // GET api/<PlaylistController>/5
        [HttpGet("{id}")]
        public Response<List<MusicListInfoDto>> GetMusicList(int id)
        {
            Response<List<MusicListInfoDto>> response = new Response<List<MusicListInfoDto>>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //checks if Musiclist exists
                    if (!_context.MusicList.Any(x => x.ID == id))
                    {
                        //list does not exist
                        response.InvalidData();
                        return response;
                    }
                    //if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    //{
                    //    //User changing the role is not an artist
                    //    response.InvalidOperation();
                    //    return response;
                    //}

                    //create a playlist variable to be checked
                    var playlist = _context.MusicList
                        .Where(x => x.ID == id)
                        .ToList();


                    List<MusicListInfoDto> playlistdetails = new List<MusicListInfoDto>();


                  
                    foreach (MusicList musiclist in playlist)
                    {
                        //find the list in list activities
                        var list = _context.TrackActivity.Where(x => x.MusicListID == musiclist.ID).ToList();

                        //create a list of tracks
                        List<PlaylistRequestDto> PlaylistTracks = new List<PlaylistRequestDto>();

                        //define the playlist
                        MusicListInfoDto listdto = new MusicListInfoDto();
                        listdto.ID = musiclist.ID;
                        listdto.Name = musiclist.ListName;


                        foreach (TrackActivity trackactivity in list)
                        {
                            //add the track to the playlist
                            Track track = _context.Track.Find(trackactivity.TrackID);
                            PlaylistRequestDto dto = new PlaylistRequestDto();
                            dto.Id = trackactivity.TrackID;
                            dto.TrackName = track.TrackName;
                            dto.TrackSource = track.TrackSource;
                            PlaylistTracks.Add(dto);
                        }
                        //add the playlist to the list of playlists
                        listdto.PlaylistTracks = PlaylistTracks;
                        playlistdetails.Add(listdto);
                    }

                    response.Success = true;
                    response.Data = playlistdetails;
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

        // POST api/<PlaylistController>
        [HttpPost]
        public Response<string> PostMusicList(PlaylistNewDto playlistnewdto)
        {
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an artist
                    //if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    //{
                    //User changing the role is not an artist
                    // response.InvalidOperation();
                    //return response;
                    //}

                    //create a new track to be inserted in DB
                    MusicList newMusicList = new MusicList();


                    //check if the new track has name of an existing track
                    var musiclistfound = _context.MusicList.Where(x => x.ListName == playlistnewdto.ListName).ToList();




                    if (musiclistfound.Count() == 1)
                    {

                        response.InvalidOperation();
                        response.Data = "Music list with the same name already exists";
                        return response;

                    }
                    else if (playlistnewdto.ListName == "")
                    {
                        response.InvalidOperation();
                        response.Data = "Music list has no name";
                        return response;
                    }
                    
                    else
                    {
                        newMusicList.ListName = playlistnewdto.ListName;
                      


                        _context.MusicList.Add(newMusicList);
                        if(_context.SaveChanges() > 0)
                        {
                            //Message was saved correctly
                            response.Success = true;
                            return response;
                        }
                        else
                        {
                            //Message was not saved correctly
                            response.ServerError();
                            response.Data = "Error while saving music list";
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

        // PUT api/<PlaylistController>/5
        [HttpPut]
        public Response<string> PutPlayList(PlaylistPutDto addtrack)
        {

            //Create a new response
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate that the requestor is an admin
                    //if (!_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"]))
                    //{
                    //User changing the role is not an admin
                    //response.InvalidOperation();
                    //return response;
                    //}

                    //Check if the musicList exists
                    if ((_context.MusicList.Any(x => x.ID == addtrack.PlayListId)))

                    {
                        List<TrackActivity> listoftracks = _context.TrackActivity.Where(x=> x.MusicListID == addtrack.PlayListId).ToList();

                        foreach(TrackPositionDto positiondto in addtrack.TrackPosition)
                        {
                            //check it the track exists and that the track is not already in the list
                            if(_context.Track.Where(x=> x.TrackID == positiondto.TrackID).Any() &&
                                !_context.TrackActivity.Where(x=> x.TrackID == positiondto.TrackID).Any())
                            {

                                TrackActivity trackactivity = new TrackActivity();
                                trackactivity.TrackID = positiondto.TrackID;
                                trackactivity.OrderNumber = positiondto.TrackPosition;
                                trackactivity.MusicListID = addtrack.PlayListId;
                                listoftracks.Add(trackactivity);


                            }
                            else
                            {
                               
                                response.InvalidData();
                                response.Data = "Track does not exist";
                                return response;

                            }
                        }

                        MusicList musiclist = _context.MusicList.Where(x => x.ID == addtrack.PlayListId).First() ;
                        musiclist.MusicTracks = listoftracks;

                        _context.Entry(musiclist).State = EntityState.Modified;

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


                        //add a track
                        //MusicList musiclist = _context.MusicList.Where(x => x.ID == addtrack.PlayListId).First();




                    }
                    else
                    {
                        //MusicList does not exist
                     
                        response.InvalidData();
                        response.Data = "Music list does not exist";
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

        // DELETE api/<PlaylistController>/5
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
                    //if (!_context.Authentication.Any(x => x.AuthenticationKey == Request.Headers["Authorization"] && x.User.Role == "artist"))
                    //{
                    //response.InvalidOperation();
                    //return response;
                    //}
                    //Validate that the track exists
                    MusicList musiclist = _context.MusicList.Find(id);
                    if (musiclist == null)
                    {
                        response.InvalidData();
                        return response;
                    }
                    var activityList = _context.TrackActivity.Where(x => x.MusicListID == id).ToList();
                    //Remove any interactions
                    foreach (TrackActivity activity in activityList)
                    {
                        _context.TrackActivity.Remove(activity);

                    }
                    //Remove any interactions
                    _context.MusicList.RemoveRange(_context.MusicList.Where(x => x == musiclist));


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
