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
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;
        private readonly DBContext _context;

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
                Stage stage = _context.UserActivity.Where(x => x.User.UserID==UserID && x.Exit == default).Include(y=>y.Stage).First().Stage;
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket,stage);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket,Stage stage)
        {
            //Create a buffer in which to store the incoming bytes
            var buffer = new byte[1024 * 4];
            //Receive the incoming message and place the individual bytes into the buffer
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            MessageSocketStartDto startdto = JsonConvert.DeserializeObject<MessageSocketStartDto>(Encoding.UTF8.GetString(buffer));
            AuthenticateKey auth = new AuthenticateKey();
           if (auth.Authenticate(_context, startdto.AuthenticationKey))
            {
                //Empty the buffer
                buffer = new byte[1024 * 4];
                //Confirm to the frontend that the connection is valid
                var AuthConfirm = Encoding.UTF8.GetBytes("Authorization passed, connection now open");
                //Send the message back to the Frontend through the webSocket
                await webSocket.SendAsync(new ArraySegment<byte>(AuthConfirm, 0, AuthConfirm.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                StageWebSocket socket = new StageWebSocket();
                socket.webSocket = webSocket;
                socket.stage = stage;
                MessageSocketManager.Instance.AddSocket(socket);
                //Enter a while loop for as long as the connection is not closed
                while (!result.CloseStatus.HasValue)
                {
                    //Receive the incoming message and place the individual bytes into the buffer
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string received = Encoding.UTF8.GetString(buffer);
                    SocketTypeReader MessageType = JsonConvert.DeserializeObject<SocketTypeReader>(received);
                    if (MessageType == null)
                    {
                        //Clear buffer as well
                        continue;
                    }

                    switch (MessageType.MessageType)
                    {
                        case "PostMessage":
                            //Try to process the message
                            try
                            {
                                //Encode the array of bytes into a string, and convert the string into a messageReceiveDto object
                                MessageReceiveDto responseObject = JsonConvert.DeserializeObject<MessageReceiveDto>(received);

                                SocketTypeWriter<Response<MessageSendDto>> response = new SocketTypeWriter<Response<MessageSendDto>>();
                                response.MessageType = "MessageResponse";
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
                                    MessageSocketManager.Instance.SendToMessageOtherClients(response.Message.Data, socket.webSocket,socket.stage);
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
                        case "PostInteraction":
                            try
                            {
                                //Encode the array of bytes into a string, and convert the string into a messageReceiveDto object
                                InteractionReceiveDto responseObject = JsonConvert.DeserializeObject<InteractionReceiveDto>(received);

                                SocketTypeWriter<Response<InteractionSendDto>> response = new SocketTypeWriter<Response<InteractionSendDto>>();
                                response.MessageType = "InteractionResponse";
                                response.Message = processInteraction(responseObject);
                                //Process the Interaction and handle the response
                                if (response != null)
                                {
                                    //Serialize the object and encode the object into an array of bytes
                                    var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                    //Send the message back to the Frontend through the webSocket
                                    await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                                }
                                if (response.Message.Success)
                                {
                                    // Find all message posted in the stage in the last 24 hours (limit 100) and count their interactions.
                                    DateTime Maxdate = DateTime.UtcNow.AddDays(-1); //Must define the max date here separately as entity framework does not allow it to be defined inside the where clause
                                    int StageID = _context.Message.Where(x => x.MessageID == responseObject.MessageID).Include(x => x.UserActivity.Stage).FirstOrDefault().UserActivity.Stage.StageID;
                                    List<Message> messages = _context.Message.Where(x => x.UserActivity.Stage.StageID == StageID & x.Timestamp > Maxdate).Take(100).ToList();
                                    List<int> InteractionTypes = _context.Interaction.Select(x => x.InteractionType).Distinct().ToList();
                                    List<MessageInteractionsDto> interactions = new List<MessageInteractionsDto>();
                                    foreach (Message message in messages)
                                    {
                                        if (_context.Interaction.Any(x => x.Message.MessageID == message.MessageID))
                                        {
                                            MessageInteractionsDto dto = new MessageInteractionsDto();
                                            dto.MessageID = message.MessageID;
                                            dto.Interactions = new List<InteractionDto>();
                                            foreach (int InteractionType in InteractionTypes)
                                            {
                                                if (_context.Interaction.Any(x => x.Message.MessageID == message.MessageID & x.InteractionType == InteractionType))
                                                {
                                                    InteractionDto intdto = new InteractionDto();
                                                    intdto.InteractionType = InteractionType;
                                                    intdto.Count = _context.Interaction.Where(x => x.Message.MessageID == message.MessageID & x.InteractionType == InteractionType).Count();
                                                    dto.Interactions.Add(intdto);
                                                }
                                            }
                                            interactions.Add(dto);
                                        }
                                    }
                                    MessageSocketManager.Instance.SendInteractionToOtherClients(interactions, socket.stage);
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
                        case "DeleteMessage":
                            try
                            {
                                int responseObject = JsonConvert.DeserializeObject<MessageIDReaderDto>(received).MessageID;

                                SocketTypeWriter<Response<string>> response = new SocketTypeWriter<Response<string>>();
                                response.MessageType = "DeleteResponse";
                                response.Message = DeleteMessage(responseObject);
                                //Process the Interaction and handle the response
                                if (response != null)
                                {
                                    //Serialize the object and encode the object into an array of bytes
                                    var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                    //Send the message back to the Frontend through the webSocket
                                    await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                                }
                                if (response.Message.Success)
                                {                                    
                                    MessageSocketManager.Instance.SendDeleteNotificationToClients(responseObject);
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
                        default:
                            try
                            {
                                //Return a default message
                                Response<string> response = new Response<string>();
                                response.RequestError();
                                response.Data = "The supplied messagetype is not recognised";
                                var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
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
                //Close the connection after exiting the while loop
                MessageSocketManager.Instance.RemoveSocket(socket);
            } else
            {
                //Confirm to the frontend that the authorization failed and close the connection
                var AuthConfirm = Encoding.UTF8.GetBytes("Authorization failed, closing the connection");
                //Send the message back to the Frontend through the webSocket
                await webSocket.SendAsync(new ArraySegment<byte>(AuthConfirm, 0, AuthConfirm.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }



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

        private Response<InteractionSendDto> processInteraction(InteractionReceiveDto Inputdto)
        {
            //Create a new response with type string
            Response<InteractionSendDto> response = new Response<InteractionSendDto>();
            try
            {
                //Find the corresponding user activity and message
                UserActivity activity = _context.UserActivity.Where(x => x.User.UserID == Inputdto.UserID && x.Exit == default).Include(x => x.User).FirstOrDefault();
                Message message = _context.Message.Find(Inputdto.MessageID);

                //Check if activity and message exist
                if(message != null & activity != null)
                {
                    //Find any previous interactions from the same user on the same message
                    var InteractionGiven = _context.Interaction.Where(x => x.Message == message && x.UserActivity.User == activity.User).FirstOrDefault();

                    if (InteractionGiven == null)
                    {
                        //If no interaction has been given, create a new interaction
                        Interaction interaction = new Interaction();
                        interaction.InteractionType = Inputdto.InteractionType;
                        interaction.UserActivity = activity;
                        interaction.Timestamp = DateTime.UtcNow;
                        interaction.Message = _context.Message.Find(Inputdto.MessageID);
                        _context.Interaction.Add(interaction);
                        if (_context.SaveChanges() > 0)
                        {
                            //Changes saved
                            response.Success = true;
                            //Create a dto
                            InteractionSendDto dto = new InteractionSendDto();
                            dto.InteractionType = interaction.InteractionType;
                            dto.Timestamp = interaction.Timestamp;
                            dto.UserName = interaction.UserActivity.User.UserName;
                            dto.Message = new MessageShortDto();
                            dto.Message.MessageID=message.MessageID;
                            dto.Message.MessageText = interaction.Message.MessageText;
                            dto.Message.Timestamp = interaction.Message.Timestamp;
                            response.Data = dto;
                            return response;
                        }
                        else
                        {
                            //Failed to save the changes
                            response.ServerError();
                            return response;
                        }
                    }
                    else
                    {
                        //If an interaction has already been given, check if the type is the same
                        if (Inputdto.InteractionType == InteractionGiven.InteractionType)
                        {
                            //If the interaction type is the same, undo the interaction
                            _context.Interaction.Remove(InteractionGiven);
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
                            //If the interaction type is different, update the existing interaction to the new interaction
                            InteractionGiven.InteractionType = Inputdto.InteractionType;
                            _context.Entry(InteractionGiven).State = EntityState.Modified;
                            if (_context.SaveChanges() > 0)
                            {
                                response.Success = true;
                                //Create a dto
                                InteractionSendDto dto = new InteractionSendDto();
                                dto.InteractionType = InteractionGiven.InteractionType;
                                dto.Timestamp = InteractionGiven.Timestamp;
                                dto.UserName = InteractionGiven.UserActivity.User.UserName;
                                dto.Message = new MessageShortDto();
                                dto.Message.MessageID = InteractionGiven.Message.MessageID;
                                dto.Message.MessageText = InteractionGiven.Message.MessageText;
                                dto.Message.Timestamp = InteractionGiven.Message.Timestamp;
                                response.Data = dto;
                                return response;
                            }
                            else
                            {
                                response.ServerError();
                                return response;
                            }
                        }
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
                response.ServerError();
                return response;
            }
        }

        private Response<string> DeleteMessage(int MessageID)
        {
            Response<string> response = new Response<string>();

            //Validate that the message exists
            Message message = _context.Message.Find(MessageID);
            if (message == null)
            {
                response.InvalidData();
                return response;
            }

            //Remove any interactions
            _context.Interaction.RemoveRange(_context.Interaction.Where(x => x.Message == message));

            //Delete the message
            if (_context.SaveChanges() > 0)
            {
                //Notify the socket of deleted message
                MessageSocketManager.Instance.SendDeleteNotificationToClients(MessageID);
                response.Success = true;
                return response;
            }
            else
            {
                response.ServerError();
                return response;
            }
        }
    }
}
