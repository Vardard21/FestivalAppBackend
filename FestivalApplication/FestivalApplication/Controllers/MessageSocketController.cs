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
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                //var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                //await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                //_logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                //_logger.Log(LogLevel.Information, "Message received from Client");
                try
                {
                   
                    string received = Encoding.UTF8.GetString(buffer);
                    MessageReceiveDto responseObject = JsonConvert.DeserializeObject<MessageReceiveDto>(received);

                    Response<string> response = processMessage(responseObject);

                    if (response != null)
                    {
                        var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                } catch (Exception exp)
                {
                    Response<System.Exception> response = new Response<System.Exception>();
                    response.ServerError();
                    response.Data = exp;
                    var responseMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                    await webSocket.SendAsync(new ArraySegment<byte>(responseMsg, 0, responseMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
                buffer = new byte[1024 * 4];
            }
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
