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
    public class MessageSocketManager
    {
        private static MessageSocketManager instance = null;
        private static readonly object padlock = new object();
        private List<StageWebSocket> ActiveSockets = new List<StageWebSocket>();

        MessageSocketManager()
        {
        }

        public static MessageSocketManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MessageSocketManager();
                    }
                    return instance;
                }
            }
        }

        public void AddSocket(StageWebSocket socket)
        {
            ActiveSockets.Add(socket);
        }

        public async void RemoveSocket(StageWebSocket webSocket)
        {
            try
            {
                await webSocket.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing the connection", CancellationToken.None);
                ActiveSockets.Remove(webSocket);
            }
            catch
            {
                ActiveSockets.Remove(webSocket);
            }
        }

        public async void SendToMessageOtherClients(MessageSendDto Message, WebSocket ParentSocket,Stage stage)
        {
            var StageActiveSockets = ActiveSockets.Where(x => x.stage.StageID == stage.StageID).ToList();
            SocketTypeWriter<MessageSendDto> SocketMessage = new SocketTypeWriter<MessageSendDto>();
            SocketMessage.MessageType = "IncomingMessage";
            SocketMessage.Message = Message;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SocketMessage));
            foreach (StageWebSocket socket in StageActiveSockets)
            {
                if(socket.webSocket.State == WebSocketState.Open && socket.webSocket != ParentSocket)
                {
                    try
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        RemoveSocket(socket);
                    }
                }
            }
        }

        public async void SendInteractionToOtherClients(List<MessageInteractionsDto> message, Stage stage)
        {
            SocketTypeWriter<List<MessageInteractionsDto>> SocketMessage = new SocketTypeWriter<List<MessageInteractionsDto>>();
            SocketMessage.MessageType = "InteractionUpdate";
            SocketMessage.Message = message;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SocketMessage));
            foreach (StageWebSocket socket in ActiveSockets)
            {
                if (socket.webSocket.State == WebSocketState.Open && socket.stage == stage)
                {
                    try
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        RemoveSocket(socket);
                    }
                }
            }
        }

        public async void SendDeleteNotificationToClients(int MessageID)
        {
            SocketTypeWriter<int> SocketMessage = new SocketTypeWriter<int>();
            SocketMessage.MessageType = "DeletedMessage";
            SocketMessage.Message = MessageID;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SocketMessage));
            foreach (StageWebSocket socket in ActiveSockets)
            {
                if (socket.webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
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
