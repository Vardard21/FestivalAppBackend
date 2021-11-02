using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FestivalApplication.Controllers;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FestivalApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StageSocketController : ControllerBase
    {
        //create a logger and a context
        private readonly ILogger<StageSocketController> _logger;
        private readonly DBContext _context;

        public StageSocketController(ILogger<StageSocketController> logger, DBContext context)
        {
            //assign the logger and the context
            _logger = logger;
            _context = context;
        }

        [HttpGet("/ws/stage/{stageID}")]
        public async Task Get(int stageID)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                //open a socket and add an instance to the list of sockets
                Stage stage = _context.Stage.Find(stageID);
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                StageSocketManager.Instance.AddSocket(webSocket, stage, GetStageUsers(stage)); 
                _logger.Log(LogLevel.Information, "WebSocket connection established");

                //enter a loop for the socket
                await Echo(webSocket, stage);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket, Stage stage)
        {
            //Create a buffer in which to store the incoming bytes
            var buffer = new byte[1024 * 4];
            //Receive the incoming message and place the individual bytes into the buffer
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            MessageSocketStartDto startdto = JsonConvert.DeserializeObject<MessageSocketStartDto>(Encoding.UTF8.GetString(buffer));
            AuthenticateKey auth = new AuthenticateKey();
            var authentication = _context.Authentication.Where(x => x.AuthenticationKey == startdto.AuthenticationKey).Include(y => y.User).FirstOrDefault();


            if (authentication != null) //check if auth key exists in the database
            {

                //Empty the buffer
                buffer = new byte[1024 * 4];
                //Confirm to the frontend that the connection is valid
                var AuthConfirm = Encoding.UTF8.GetBytes("Stage Socket: Authorization passed, connection now open");
                //Send the message back to the Frontend through the webSocket
                await webSocket.SendAsync(new ArraySegment<byte>(AuthConfirm, 0, AuthConfirm.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                StageSocketManager.Instance.UpdateUserList(stage, GetStageUsers(stage));

                var onloaddto = OnLoadTrack(stage.StageID);
                var onloadresponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(onloaddto));
                await webSocket.SendAsync(new ArraySegment<byte>(onloadresponseMsg, 0, onloadresponseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                //validate if auth key is an artist
                if (_context.Authentication.Where(x => x.User.Role == "artist" && x.AuthenticationKey == authentication.AuthenticationKey).Any())
                {

                    //var ArtistDetected = Encoding.UTF8.GetBytes("Artist Detected, Sending Available Tracks Now ");
                    ////Send the message back to the Frontend through the webSocket
                    //await webSocket.SendAsync(new ArraySegment<byte>(ArtistDetected, 0, ArtistDetected.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    //create a list of tracks to be sent
                    StageSocketWriterDto<List<MusicListInfoDto>> adto = new StageSocketWriterDto<List<MusicListInfoDto>>();
                    adto.StageData = GetTracks();
                    adto.StageCase = "ArtistGetList";
                    var encodedmusiclists = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(adto));
                    await webSocket.SendAsync(new ArraySegment<byte>(encodedmusiclists, 0, encodedmusiclists.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    //find the user thats connected to the auth key and check in which stage said user is
                    User user = authentication.User;
                    //Enter a while loop for as long as the connection is not closed
                    while (!result.CloseStatus.HasValue)
                    {
                        //empty buffer
                        buffer = new byte[4 * 1024];
                        //Recieve the incoming TrackID and place the individual bytes into the buffer
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        string received = Encoding.UTF8.GetString(buffer);

                        //convert byte array to json
                        var incomingjson = JsonConvert.DeserializeObject<PlaylistReceiveDto>(received);
                        StageSocketWriterDto<PlaylistUpdateDto> dto = new StageSocketWriterDto<PlaylistUpdateDto>();
                        if (incomingjson != null)
                        {
                            try
                            {
                                if (incomingjson.ReceivedCase == "SongSelection")
                                {
                                    //create a new dto to send to frontend


                                    //Select new tracks and add that to the dto

                                    //var TrackSelected = Encoding.UTF8.GetBytes("Track Selected, Playing Now ");
                                    ////Send the message back to the Frontend through the webSocket
                                    //await webSocket.SendAsync(new ArraySegment<byte>(TrackSelected, 0, TrackSelected.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                                    dto.StageData = SelectNewTrack(incomingjson.TrackID, incomingjson.PlaylistID, authentication, stage.StageID);
                                    dto.StageCase = "ArtistSelection";


                                    //Serialize the object and encode the object into an array of bytes
                                    var encodedResponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));

                                    //Send the message back to the Frontend through the webSocket
                                    await webSocket.SendAsync(new ArraySegment<byte>(encodedResponseMsg, 0, encodedResponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                                    //Check if the message is not empty
                                    if (dto.StageData != null)
                                    {
                                        //Send newly selected song to other clients
                                        StageSocketManager.Instance.SendToTrackOtherClients(dto, webSocket, stage, GetStageUsers(stage),_context);
                                    }
                                }
                                else if (incomingjson.ReceivedCase == "SongPause")
                                {

                                    var currentList =_context.MusicListActivity.Where(x=>x.StageID==stage.StageID).First();
                                    var trackActivity = _context.TrackActivity.Where(x => x.MusicListID == currentList.ListID).First();
                                    dto.StageData = SelectNewTrack(trackActivity.TrackID, currentList.ListID, authentication, stage.StageID);
                                    dto.StageCase = incomingjson.ReceivedCase;
                                    //Serialize the object and encode the object into an array of bytes
                                    var encodedResponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));

                                    //Send the message back to the Frontend through the webSocket
                                    await webSocket.SendAsync(new ArraySegment<byte>(encodedResponseMsg, 0, encodedResponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                                    //Check if the message is not empty
                                    if (dto.StageData != null)
                                    {
                                        //Send newly selected song to other clients
                                        StageSocketManager.Instance.SendToTrackOtherClients(dto, webSocket, stage, GetStageUsers(stage),_context);
                                    }
                                }
                                else if (incomingjson.ReceivedCase == "SongResume")
                                {
                                    var currentList = _context.MusicListActivity.Where(x => x.StageID == stage.StageID).First().ListID;
                                    var trackActivity = _context.TrackActivity.Where(x => x.MusicListID == currentList).First().TrackID;
                                    dto.StageData = SelectNewTrack(trackActivity, currentList, authentication, stage.StageID, incomingjson.SongTime);
                                    dto.StageCase = incomingjson.ReceivedCase;



                                    //Serialize the object and encode the object into an array of bytes
                                    var encodedResponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));

                                    //Send the message back to the Frontend through the webSocket
                                    await webSocket.SendAsync(new ArraySegment<byte>(encodedResponseMsg, 0, encodedResponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                                    //Check if the message is not empty
                                    if (dto.StageData != null)
                                    {
                                        //Send newly selected song to other clients
                                        StageSocketManager.Instance.SendToTrackOtherClients(dto, webSocket, stage, GetStageUsers(stage),_context);
                                    }
                                }



                            }
                            catch (Exception exp)
                            {
                                //Send back a response with the exception
                                Response<System.Exception> response = new Response<System.Exception>();
                                response.ServerError();
                                response.Data = exp;
                                //Serialize the object and encode the object into an array of bytes
                                var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                //Send the message back to the Frontend through the webSocket
                                await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                            }


                        }
                        else
                        {
                            dto.StageData = null;
                            dto.StageCase = "Failed";
                            //Serialize the object and encode the object into an array of bytes
                            var encodedResponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));

                            //Send the message back to the Frontend through the webSocket
                            await webSocket.SendAsync(new ArraySegment<byte>(encodedResponseMsg, 0, encodedResponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                        }

                    }
                }
                else
                {

                    {   //keep connection open but send nothing if not artist
                        while (!result.CloseStatus.HasValue)
                        {
                            buffer = new byte[4 * 1024];

                        }
                    }

                }
            }
            //Close the connection when requested
            StageSocketManager.Instance.UpdateUserList(stage, GetStageUsers(stage));
            StageSocketManager.Instance.RemoveSocket(webSocket, stage,GetStageUsers(stage));

        }
        private Response<PlaylistUpdateDto> SelectNewTrack(int id, int musicid, Authentication authentication,int StageID)
        {
            //create a response to send back
            Response<PlaylistUpdateDto> response = new Response<PlaylistUpdateDto>();
            try
            {

                //check if user is actually an artist 
                if (!_context.Authentication.Where(x => x.User.Role == "artist" && x.AuthenticationKey == authentication.AuthenticationKey).Any())
                {
                    //User is not an artist
                    response.InvalidOperation();
                    return response;
                }

                // check if the playlist exists
                if (!(_context.MusicList.Where(x => x.ID == musicid).Count() == 1))
                {
                    //Playlist does not exist
                    response.InvalidData();
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
                var selectedtrack = _context.TrackActivity.Where(x => x.TrackID == id && x.MusicListID == musicid).ToList();

                //check if selected track only has 1 entry
                if (selectedtrack.Count() == 1)
                {
                    foreach (TrackActivity trackactivity in selectedtrack)
                    {
                        {
                            //update the required track in the playlist
                            trackactivity.Playing = true;
                            Track track = _context.Track.Find(trackactivity.TrackID);
                            PlaylistUpdateDto dto = new PlaylistUpdateDto();
                            dto.TrackName = track.TrackName;
                            dto.TrackSource = track.TrackSource;
                            dto.SongTime = 0;
                            dto.Playing = true;
                            dto.PlayListID = musicid;
                            dto.TrackID = track.TrackID;
                            UpdateMusiclistActivity(StageID, musicid, track.TrackID);

                            //save trackactivity
                            _context.Entry(trackactivity).State = EntityState.Modified;
                            if (_context.SaveChanges() > 0)
                            {
                                //Track has been set to playing
                                response.Success = true;
                                response.Data = dto;
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
                    response.Data.TrackName = "Multiple Playlists found";
                    return response;
                }
                return response;
            }
            catch
            {
                response.ServerError();
                return response;
            }
        }
        private Response<PlaylistUpdateDto> SelectNewTrack(int id, int musicid, Authentication authentication, int StageID,double SongTime)
        {
            //create a response to send back
            Response<PlaylistUpdateDto> response = new Response<PlaylistUpdateDto>();
            try
            {

                //check if user is actually an artist 
                if (!_context.Authentication.Where(x => x.User.Role == "artist" && x.AuthenticationKey == authentication.AuthenticationKey).Any())
                {
                    //User is not an artist
                    response.InvalidOperation();
                    return response;
                }

                // check if the playlist exists
                if (!(_context.MusicList.Where(x => x.ID == musicid).Count() == 1))
                {
                    //Playlist does not exist
                    response.InvalidData();
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
                var selectedtrack = _context.TrackActivity.Where(x => x.TrackID == id && x.MusicListID == musicid).ToList();

                //check if selected track only has 1 entry
                if (selectedtrack.Count() == 1)
                {
                    foreach (TrackActivity trackactivity in selectedtrack)
                    {
                        {
                            //update the required track in the playlist
                            trackactivity.Playing = true;
                            Track track = _context.Track.Where(x=>x.TrackID==trackactivity.TrackID).First();
                            PlaylistUpdateDto dto = new PlaylistUpdateDto();
                            dto.TrackName = track.TrackName;
                            dto.TrackID = track.TrackID;
                            dto.TrackSource = track.TrackSource;
                            dto.SongTime = SongTime;
                            dto.PlayListID =musicid;
                            dto.Playing = true;

                            //save trackactivity
                            _context.Entry(trackactivity).State = EntityState.Modified;
                            if (_context.SaveChanges() > 0)
                            {
                                //Track has been set to playing
                                response.Success = true;
                                response.Data = dto;
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
                    response.Data.TrackName = "Multiple Playlists found";
                    return response;
                }
                return response;
            }
            catch
            {
                response.ServerError();
                return response;
            }
        }

        private Response<List<MusicListInfoDto>> GetTracks()
        {
            //create a response to send
            Response<List<MusicListInfoDto>> response = new Response<List<MusicListInfoDto>>();

            //create a list of playlists and tracks to add to the response
            List<MusicListInfoDto> MusicAndTracks = new List<MusicListInfoDto>();
            try
            {
                //create a list of tracks to add to each musiclist
                var MusicLists = _context.MusicList.ToList();

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
                        Track track = _context.Track.Where(x=>x.TrackID==trackactivity.TrackID).First();
                        PlaylistRequestDto dto = new PlaylistRequestDto();
                        dto.Id = trackactivity.TrackID;
                        dto.TrackName = track.TrackName;
                        dto.TrackSource = track.TrackSource;
                        PlaylistTracks.Add(dto);
                    }
                    //add the playlist to the list of playlists
                    listdto.PlaylistTracks = PlaylistTracks;
                    MusicAndTracks.Add(listdto);
                }
                //add the list of playlists to the response and send it back
                response.Success = true;
                response.Data = MusicAndTracks;
                return response;
            }
            catch
            {
                //server error
                response.ServerError();
                return response;
            }
        }

        private Response<int> UpdateMusiclistActivity(int StageID, int MusiclistID, int TrackID)
        {   
            //Create the response to be send out
            Response<int> response = new Response<int>();
            try {

                //If the stageID is not 0, validate that the StageID exists and the stage is active
                if (StageID != 0)
                {
                    if (_context.Stage.Find(StageID).StageActive != true)
                    {
                        response.InvalidOperation();
                        return response;
                    }

                    if (_context.MusicListActivity.Any(x => x.StageID == StageID && x.ListID == MusiclistID))
                    {
                        //check if list exists for stage already
                        if (_context.MusicListActivity.Where(x => x.StageID == StageID&& x.StageID!=MusiclistID).Any())
                        {
                            var ExistingActivities = _context.MusicListActivity.Where(x => x.StageID == StageID && x.StageID != MusiclistID).ToList();
                            foreach (MusicListActivity musicListactivity in ExistingActivities)
                            {
                                //delete existing activities for stage
                                _context.MusicListActivity.Remove(musicListactivity);
                            }
                        }
                        var activelist = _context.MusicListActivity.Where(x => x.StageID == StageID && x.ListID == MusiclistID).First();
                        activelist.ListID = MusiclistID;
                        activelist.StageID = StageID;
                        activelist.TrackID = TrackID;
                        _context.Entry(activelist).State = EntityState.Modified;
                    }
                    else
                    {
                        //check if list exists for stage already
                        if (_context.MusicListActivity.Where(x => x.StageID == StageID).Any())
                        {
                            var ExistingActivities = _context.MusicListActivity.Where(x => x.StageID == StageID).ToList();
                            foreach (MusicListActivity musicListactivity in ExistingActivities)
                            {
                                //delete existing activities for stage
                                _context.MusicListActivity.Remove(musicListactivity);
                            }
                        }
                        //Create a new UserActivity for this UserID and StageID
                        MusicListActivity activity = new MusicListActivity();
                        activity.ListID = MusiclistID;
                        activity.StageID = StageID;
                        activity.TrackID = TrackID;
                        _context.MusicListActivity.Add(activity);
                    }
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
                        response.ServerError();
                        return response;
                    }


                }
                else
                {
                    response.InvalidOperation();
                    return response;
                }
            }
            catch
            {
                response.InvalidOperation();
                Console.WriteLine("catcherror");
                return response;
            }
        }

        private Response<List<StageUsersDto>> GetStageUsers(Stage stage)
        {

            //creates a response variable to be sent
            Response<List<StageUsersDto>> response = new Response<List<StageUsersDto>>();
            try
            {
                //checks if stage exists
                if (!_context.Stage.Any(x => x.StageID == stage.StageID))
                {
                    //Stage does not exist
                    response.InvalidData();
                    return response;
                }

                //create a stages variable to be checked
                var stageusers = _context.UserActivity
                    .Where(x => x.Stage.StageID == stage.StageID && x.Exit == default)
                    .Include(y => y.User)
                    .ToList();

                List<StageUsersDto> ActiveUsers = new List<StageUsersDto>();

                if (!ActiveUsers.Any())
                {

                    //create a for loop for each stage in stage status
                    foreach (UserActivity useractivity in stageusers)
                    {
                        //Create a new Stage Request DTO and fill the id and name
                        StageUsersDto dto = new StageUsersDto();
                        User user = _context.User.Find(useractivity.User.UserID);
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
                    response.ServerError();
                    return response;
                }

            }
            catch
            {
                response.ServerError();
                return response;
            }
        }

        private StageSocketWriterDto<PlaylistUpdateDto> OnLoadTrack(int StageID)
        {
            StageSocketWriterDto<PlaylistUpdateDto> dto = new StageSocketWriterDto<PlaylistUpdateDto>();
            int trackid = _context.MusicListActivity.Where(x => x.StageID ==StageID ).First().TrackID;
            Track track = _context.Track.Find(trackid);
            Response<PlaylistUpdateDto> response = new Response<PlaylistUpdateDto>();

            PlaylistUpdateDto trackdto = new PlaylistUpdateDto();
            trackdto.TrackName = track.TrackName;
            trackdto.TrackSource = track.TrackSource;
            response.Success = true;
            response.Data = trackdto;
            dto.StageData = response;
            dto.StageCase = "OnLoadTrack";
            return dto;
        }
    }

} 



