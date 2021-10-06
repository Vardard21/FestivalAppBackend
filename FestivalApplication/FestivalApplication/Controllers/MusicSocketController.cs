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
        private readonly ILogger<MusicSocketController> _logger;
        private readonly DBContext _context;
        private List<WebSocket> ActiveSockets = new List<WebSocket>();
        public MusicSocketController(ILogger<MusicSocketController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/wsm")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
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
            var buffer = new byte[4];
            //Recieve the incoming URL and place the individual bytes into the buffer
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "New song received from Client");


            //Enter a while loop for as long as the connection is not closed
            while (!result.CloseStatus.HasValue)
            {
                //Recieve the incoming TrackID and place the individual bytes into the buffer
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string received = Encoding.UTF8.GetString(buffer);
                //initialize 2 integers to send to SelectNewTrack
                int receivedint1;
                int receivedint2;

                // convert the recieved string to the above 2 integers 
                if (int.TryParse(received, out int parsed))
                {
                    //grab the first digit
                    int i = Math.Abs(parsed);
                    while(i>=10)
                    { i /= 10; }
                    receivedint1 = i;
                    //grab the last digit
                    int j = Math.Abs(parsed) % 10;
                    receivedint2 = j;
                }
                else
                {
                      receivedint1 = 1;
                      receivedint2 = 1; 
                }
                
                //Serialize the object and encode the object into an array of bytes
                var responseMsg = SelectNewTrack(receivedint1, receivedint2).Data;
                var encodedresponseMsg= Encoding.UTF8.GetBytes(responseMsg.TrackName.ToString());
                //Send the message back to the Frontend through the webSocket
                await webSocket.SendAsync(new ArraySegment<byte>(encodedresponseMsg, 0, encodedresponseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                if (responseMsg!=null)
                { 
                    MusicSocketManager.Instance.SendToTrackOtherClients(responseMsg, webSocket);
                }

                //Empty the buffer
                buffer = new byte[1024 * 4];

            }
            //Close the connection when requested
            MusicSocketManager.Instance.RemoveSocket(webSocket);
        }
        private Response<PlaylistSendDto> SelectNewTrack(int id, int musicid)
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
                    var selectedtrack = _context.TrackActivity.Where(x => x.TrackID == id).ToList();

                    //check if selected track only has 1 entry
                    if (selectedtrack.Count() == 1)
                    {
                        foreach (TrackActivity trackactivity in selectedtrack)
                        {
                            {
                                trackactivity.Playing = true;
                                Track track = _context.Track.Find(trackactivity.TrackID);
                                PlaylistSendDto dto = new PlaylistSendDto();
                                dto.TrackName = track.TrackName;


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

