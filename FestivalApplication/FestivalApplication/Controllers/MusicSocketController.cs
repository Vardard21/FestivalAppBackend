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
    public class MusicSocketController : ControllerBase
    {
        //create a logger and a context
        private readonly ILogger<MusicSocketController> _logger;
        private readonly DBContext _context;

        public MusicSocketController(ILogger<MusicSocketController> logger, DBContext context)
        {
            //assign the logger and the context
            _logger = logger;
            _context = context;
        }

        [HttpGet("/ws/{StageID}")]
        public async Task Get(int StageID)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                //open a socket and add an instance to the list of sockets
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                MusicSocketManager.Instance.AddSocket(webSocket);
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            //Create a buffer in which to store the incoming bytes
            var buffer = new byte[4 * 1024];
            //Recieve the incoming Auth-key and place the individual bytes into the buffer
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            //Convert the auth key from byte to string
            Authentication key = JsonConvert.DeserializeObject<Authentication>(Encoding.UTF8.GetString(buffer));
            AuthenticateKey auth = new AuthenticateKey();
            if (auth.Authenticate(_context, key.AuthenticationKey)) //check if auth key exists in the database
            {
                //Add a new socket to the instance
                MusicSocketManager.Instance.AddSocket(webSocket);

                //validate if auth key is an artist
                if (_context.Authentication.Any(x => x.User.Role == "artist" && x.AuthenticationKey == Request.Headers["Authorization"])) 
                {
                    //create a list of tracks to be sent
                    var encodedmusiclists = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetTracks())); 
                    await webSocket.SendAsync(new ArraySegment<byte>(encodedmusiclists, 0, encodedmusiclists.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    //Enter a while loop for as long as the connection is not closed
                    while (!result.CloseStatus.HasValue)
                    {
                        //Recieve the incoming TrackID and place the individual bytes into the buffer
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        string received = Encoding.UTF8.GetString(buffer);

                        //convert byte array to json
                        var incomingjson = JsonConvert.DeserializeObject<PlaylistReceiveDto>(received);
                        var strInput = received.Trim();

                        //check if the object is a valid JSON
                        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                            (strInput.StartsWith("[") && strInput.EndsWith("]")))    //For array
                        {
                            try
                            {
                                //Serialize the object and encode the object into an array of bytes
                                var responseMsg = SelectNewTrack(incomingjson.TrackID, incomingjson.PlaylistID);
                                var encodedresponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseMsg));

                                //Send the message back to the Frontend through the webSocket
                                await webSocket.SendAsync(new ArraySegment<byte>(encodedresponseMsg, 0, encodedresponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                                //Check if the message is not empty
                                if (responseMsg != null)
                                {
                                    //Send newly selected song to other clients
                                    MusicSocketManager.Instance.SendToTrackOtherClients(responseMsg, webSocket,incomingjson.StageID);
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
                            continue;
                        }

                    }
                }
                else
                {   //keep connection open but send nothing if not artist
                    while (!result.CloseStatus.HasValue)
                    {
                        buffer = new byte[4 * 1024];
                    }
                }

            }
            //Close the connection when requested
            MusicSocketManager.Instance.RemoveSocket(webSocket);
        }
        private Response<PlaylistUpdateDto> SelectNewTrack(int id, int musicid)
        {
            //create a response to send back
            Response<PlaylistUpdateDto> response = new Response<PlaylistUpdateDto>();
            try
            {

                //check if user is actually an artist 
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
                    //Playlist does not exist
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
                    Console.WriteLine("Multiple Playlist found");
                    return response;
                }

                return response;



            }
            catch
            {
                response.ServerError();
                Console.WriteLine("Server Error");
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
                        Track track = _context.Track.Find(trackactivity.TrackID);
                        PlaylistRequestDto dto = new PlaylistRequestDto();
                        dto.Id = trackactivity.TrackID;
                        dto.TrackName = track.TrackName;
                        dto.TrackSource = track.TrackSource;
                        dto.Length = track.Length;
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

    }

}



