using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
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

        public WebSocketsController(ILogger<WebSocketsController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
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

                //Try to process the message
                try
                {
                    //Encode the array of bytes into a string, and convert the string into a messageReceiveDto object
                    string received = Encoding.UTF8.GetString(buffer);
                    MessageReceiveDto responseObject = JsonConvert.DeserializeObject<MessageReceiveDto>(received);

                    //Process the message and handle the response
                    Response<string> response = processMessage(responseObject);
                    if (response != null)
                    {
                        //Serialize the object and encode the object into an array of bytes
                        var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        //Send the message back to the Frontend through the webSocket
                        await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                } catch (Exception exp)
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
                //Empty the buffer
                buffer = new byte[1024 * 4];
            }
            //Close the connection when requested
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }

        private Response<string> processMessage(MessageReceiveDto messagedto)
        {
            //Create a new response with type string
            Response<string> response = new Response<string>();
            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (!auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Create a new message to transfer the message data into
                    Message message = new Message();
                    //Find UserActivities currently active for the UserID
                    var activitiesfound = _context.UserActivity.Where(x => x.UserID == messagedto.UserID && x.Exit == default).ToList();

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
                            return response;
                        }
                        else
                        {
                            //Message was not saved correctly
                            response.Success = false;
                            response.ErrorMessage.Add(1);
                            return response;
                        }
                    }
                    else
                    {
                        //There was no active UserActivity for this user
                        response.Success = false;
                        response.ErrorMessage.Add(3);
                        return response;
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage.Add(5);
                    return response;
                }
            }
            catch
            {
                response.Success = false;
                response.ErrorMessage.Add(1);
                return response;
            }
        }
    }
}
