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
        private List<WebSocket> ActiveSockets = new List<WebSocket>();

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

        public async void SendToMessageOtherClients(MessageSendDto Message, WebSocket ParentSocket)
        {
            SocketTypeWriter<MessageSendDto> SocketMessage = new SocketTypeWriter<MessageSendDto>();
            SocketMessage.MessageType = "IncomingMessage";
            SocketMessage.Message = Message;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SocketMessage));
            foreach (WebSocket socket in ActiveSockets)
            {
                if(socket.State == WebSocketState.Open & socket != ParentSocket)
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

        public async void SendInteractionToOtherClients(List<MessageInteractionsDto> message)
        {
            SocketTypeWriter<List<MessageInteractionsDto>> SocketMessage = new SocketTypeWriter<List<MessageInteractionsDto>>();
            SocketMessage.MessageType = "InteractionUpdate";
            SocketMessage.Message = message;
            var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SocketMessage));
            foreach (WebSocket socket in ActiveSockets)
            {
                if (socket.State == WebSocketState.Open)
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
