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
    public class MusicSocketManager
    {
        private static MusicSocketManager instance = null;
        private static readonly object padlock = new object();
        private List<WebSocket> ActiveSockets = new List<WebSocket>();

        MusicSocketManager()
        {
        }

        public static MusicSocketManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MusicSocketManager();
                    }
                    return instance;
                }
            }
        }

        public void AddSocket(WebSocket webSocket)
        {
            ActiveSockets.Add(webSocket);
        }

        public async void RemoveSocket(WebSocket webSocket)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing the connection", CancellationToken.None);
                ActiveSockets.Remove(webSocket);
            }
            catch
            {
                ActiveSockets.Remove(webSocket);
            }
        }

        public async void SendToTrackOtherClients(Response<PlaylistUpdateDto> track, WebSocket ParentSocket, int StageID)
        {
            SocketTypeWriter<PlaylistUpdateDto> SocketMessage = new SocketTypeWriter<PlaylistUpdateDto>();
            SocketMessage.MessageType = "IncomingTrack";
            SocketMessage.Message = track.Data;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(track));
            foreach (WebSocket socket in ActiveSockets)
            {
                if (socket.State != WebSocketState.Open & socket.State != WebSocketState.Connecting)
                {
                    RemoveSocket(socket);
                }
                if (socket != ParentSocket && track.Success==true)
                {
                    try
                    {
                        await socket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        RemoveSocket(socket);
                    }
                }
            }
        }
    }
}
