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

namespace WebSocketsTutorial.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;
        private readonly DBContext _context;
        private List<WebSocket> ActiveSockets = new List<WebSocket>();

        public WebSocketsController(ILogger<WebSocketsController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/ws/{UserID}")]
        public async Task Get(int UserID)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                MessageSocketManager.Instance.AddSocket(webSocket);
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
            var buffer = new byte[1024 * 4];
            //Receive the incoming message and place the individual bytes into the buffer
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            //Enter a while loop for as long as the connection is not closed
            while (!result.CloseStatus.HasValue)
            {
                //Receive the incoming message and place the individual bytes into the buffer
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string received = Encoding.UTF8.GetString(buffer);
                SocketTypeReader MessageType = JsonConvert.DeserializeObject<SocketTypeReader>(received);
                if(MessageType == null)
                {
                    //Clear buffer as well
                    continue;
                }

                switch (MessageType.MessageType)
                {
                    case "NewMessage":
                        //Try to process the message
                        try
                        {
                            //Encode the array of bytes into a string, and convert the string into a messageReceiveDto object
                            MessageReceiveDto responseObject = JsonConvert.DeserializeObject<MessageReceiveDto>(received);

                            SocketTypeWriter<Response<MessageSendDto>> response = new SocketTypeWriter<Response<MessageSendDto>>();
                            response.MessageType = "PostResponse";
                            response.Message = processMessage(responseObject);
                            //Process the message and handle the response
                            if (response != null)
                            {
                                //Serialize the object and encode the object into an array of bytes
                                var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                //Send the message back to the Frontend through the webSocket
                                await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                            }
                            if (response.Message.Success)
                            {
                                MessageSocketManager.Instance.SendToMessageOtherClients(response.Message.Data, webSocket);
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
                        break;
                }
                //Empty the buffer
                buffer = new byte[1024 * 4];
            }
            //Close the connection when requested
            MessageSocketManager.Instance.RemoveSocket(webSocket);
        }

        private Response<MessageSendDto> processMessage(MessageReceiveDto messagedto)
        {
            //Create a new response with type string
            Response<MessageSendDto> response = new Response<MessageSendDto>();
            try
            {
                //Create a new message to transfer the message data into
                Message message = new Message();

                //Find the user associated with the message
                User Author = _context.User.Find(messagedto.UserID);
                if(Author == null)
                {
                    response.InvalidData();
                    return response;
                }

                //Find UserActivities currently active for the UserID
                var activitiesfound = _context.UserActivity.Where(x => x.User == Author && x.Exit == default).ToList();

                //Check if the user is currently active in an activity
                if (activitiesfound.Count() == 1)
                {
                    //Insert message text, UserActivity and timestamp into message object
                    message.UserActivity = activitiesfound[0];
                    message.MessageText = messagedto.MessageText;
                    message.Timestamp = DateTime.UtcNow;

                    //Save the message object
                    _context.Message.Add(message);
                    if (_context.SaveChanges() > 0)
                    {
                        //Message was saved correctly
                        response.Success = true;
                        //Convert the message to a response Dto object
                        MessageSendDto dto = new MessageSendDto();
                        dto.MessageID = message.MessageID;
                        dto.MessageText = message.MessageText;
                        dto.Timestamp = message.Timestamp;
                        dto.UserName = Author.UserName;
                        dto.UserRole = Author.Role;
                        response.Data = dto;
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
                    //There was no active UserActivity for this user
                    response.InvalidOperation();
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
