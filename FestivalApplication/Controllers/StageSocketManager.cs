using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FestivalApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FestivalApplication.Controllers
{
    public class StageSocketManager: ControllerBase
    {
        private static StageSocketManager instance = null;
        private static readonly object padlock = new object();
        private List<StageWebSocket> ActiveSockets = new List<StageWebSocket>();

        StageSocketManager()
        {
        }

        public static StageSocketManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new StageSocketManager();
                    }
                    return instance;
                }
            }
        }

        public void AddSocket(WebSocket webSocket, Stage stage, Response<List<StageUsersDto>> response)
        {
            StageWebSocket stageWebSocket = new StageWebSocket();
            stageWebSocket.webSocket = webSocket;
            stageWebSocket.stage = stage;
            ActiveSockets.Add(stageWebSocket);
        }

        public async void RemoveSocket(WebSocket webSocket, Stage stage, Response<List<StageUsersDto>> response)
        {
            try
            {
                StageWebSocket stageWebSocket = new StageWebSocket();
                stageWebSocket.webSocket = webSocket;
                stageWebSocket.stage = stage;
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing the connection", CancellationToken.None);               
                ActiveSockets.Remove(stageWebSocket);
            }
            catch
            {
                StageWebSocket stageWebSocket = new StageWebSocket();
                stageWebSocket.webSocket = webSocket;
                stageWebSocket.stage = stage;
                ActiveSockets.Remove(stageWebSocket);
            }
        }
        public async void UpdateUserList(Stage stage, Response<List<StageUsersDto>> response)
        {
            var StageActiveSockets = ActiveSockets.Where(x => x.stage.StageID == stage.StageID).ToList();
            StageSocketWriterDto<List<StageUsersDto>> udto = new StageSocketWriterDto<List<StageUsersDto>>();
            udto.StageData = response;
            udto.StageCase = "UpdatedUserList";
            var userResponseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(udto));

            foreach (StageWebSocket socket in StageActiveSockets)
            {
                if (socket.webSocket.State != WebSocketState.Open & socket.webSocket.State != WebSocketState.Connecting)
                {
                    RemoveSocket(socket.webSocket, socket.stage, response);
                }
                try
                {
                    await socket.webSocket.SendAsync(new ArraySegment<byte>(userResponseMsg, 0, userResponseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    RemoveSocket(socket.webSocket, socket.stage, response);
                }
            }

        }
 
        public async void SendToTrackOtherClients(StageSocketWriterDto<PlaylistUpdateDto> track, WebSocket ParentSocket, Stage stage, Response<List<StageUsersDto>> response,DBContext context)
        {
            var StageActiveSockets = ActiveSockets.Where(x => x.stage.StageID == stage.StageID).ToList();
            StageSocketWriterDto<PlaylistUpdateDto> dto = new StageSocketWriterDto<PlaylistUpdateDto>();
            Response<PlaylistUpdateDto> OutgoingSignal = new Response<PlaylistUpdateDto>();
            dto.StageCase="Test";
            if (track.StageCase == "ArtistSelection")
            {
                track.StageData.Data.SongTime = 0;
                track.StageData.Data.Playing = true;
                dto.StageCase = "IncomingTrack";
            }
            else if (track.StageCase == "SongPause")
            {
                track.StageData.Data.Playing = false;
                dto.StageCase = "SongPause";
            }
            else if (track.StageCase == "SongResume")
            {
                track.StageData.Data.SongTime = track.StageData.Data.SongTime;
                track.StageData.Data.Playing = true;
                dto.StageCase = "SongResume";
            }
            OutgoingSignal = track.StageData;
            dto.StageData = OutgoingSignal;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));


            foreach (StageWebSocket socket in StageActiveSockets)
            {
                if (socket.webSocket.State != WebSocketState.Open & socket.webSocket.State != WebSocketState.Connecting)
                {
                    RemoveSocket(socket.webSocket, socket.stage, response);
                    UpdateUserList(stage, GetStageUsers(stage, context));
                }
                if (socket.webSocket != ParentSocket && OutgoingSignal.Success == true)
                {
                    try
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        RemoveSocket(socket.webSocket, socket.stage, response);
                    }
                }
            }
            
        }
        private Response<List<StageUsersDto>> GetStageUsers(Stage stage, DBContext context)
        {

            //creates a response variable to be sent
            Response<List<StageUsersDto>> response = new Response<List<StageUsersDto>>();
            try
            {
                //checks if stage exists
                if (!context.Stage.Any(x => x.StageID == stage.StageID))
                {
                    //Stage does not exist
                    response.InvalidData();
                    return response;
                }

                //create a stages variable to be checked
                var stageusers = context.UserActivity
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
                        User user = context.User.Find(useractivity.User.UserID);
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
    }
}
