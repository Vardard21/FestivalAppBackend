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


namespace FestivalApplication.Controllers
{
    public class StageSocketManager
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

        public void AddSocket(WebSocket webSocket, Stage stage)
        {
            StageWebSocket stageWebSocket = new StageWebSocket();
            stageWebSocket.webSocket = webSocket;
            stageWebSocket.stage = stage;
            ActiveSockets.Add(stageWebSocket);
        }

        public async void RemoveSocket(WebSocket webSocket, Stage stage)
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

        public async void SendToTrackOtherClients(Response<PlaylistUpdateDto> track, WebSocket ParentSocket, Stage stage)
        {
            var StageActiveSockets = ActiveSockets.Where(x => x.stage.StageID == stage.StageID).ToList();
            StageSocketWriterDto<PlaylistUpdateDto> dto = new StageSocketWriterDto<PlaylistUpdateDto>();
            dto.StageData = track;
            dto.StageCase = "IncomingTrack"+" This many sockets: "+StageActiveSockets.Count();
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto));


            foreach (StageWebSocket socket in StageActiveSockets)
            {
                if (socket.webSocket.State != WebSocketState.Open & socket.webSocket.State != WebSocketState.Connecting)
                {
                    RemoveSocket(socket.webSocket, socket.stage);
                }
                if (socket.webSocket != ParentSocket && track.Success == true)
                {
                    try
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        RemoveSocket(socket.webSocket, socket.stage);
                    }
                }
            }
        }
    }
}
