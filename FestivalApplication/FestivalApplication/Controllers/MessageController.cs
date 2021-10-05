﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;

namespace FestivalApplication.Controllers
{
    [Route("api/Messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly DBContext _context;

        public MessageController(DBContext context)
        {
            _context = context;
        }

        // GET: api/Message
        [HttpPut]
        public Response<List<MessageSendDto>> GetMessage(ChatHistoryDto historyrequest)
        {
            Response<List<MessageSendDto>> response = new Response<List<MessageSendDto>>();

            try
            {
                AuthenticateKey auth = new AuthenticateKey();
                if (!auth.Authenticate(_context, Request.Headers["Authorization"]))
                {
                    //Validate LastUpdated date and set default to one hour ago
                    if (historyrequest.LastUpdated < DateTime.UtcNow.AddHours(-1))
                    {
                        historyrequest.LastUpdated = DateTime.UtcNow.AddHours(-1);
                    }

                    //Check for all messages posted in the stage since the last update date
                    var MessageHistory = _context.Message.Where(x => x.Timestamp > historyrequest.LastUpdated && x.UserActivity.StageID == historyrequest.StageID).Include(message => message.UserActivity).ToList();

                    //Initialize the return list
                    List<MessageSendDto> ChatHistory = new List<MessageSendDto>();

                    //Convert Message objects to MessageSendDto objects and populate the return list
                    foreach (Message message in MessageHistory)
                    {
                        //Create a new Send Message Object and fill the text and timestamp
                        MessageSendDto dto = new MessageSendDto();
                        dto.MessageText = message.MessageText;
                        dto.Timestamp = message.Timestamp;
                        //Find the author by UserID and assign the UserName and UserRole
                        User Author = _context.User.Find(message.UserActivity.UserID);
                        dto.UserName = Author.UserName;
                        dto.UserRole = Author.Role;
                        //Add the new object to the return list
                        ChatHistory.Add(dto);
                    }

                    //Return the Dto list
                    response.Success = true;
                    response.Data = ChatHistory;
                    return response;
                } else
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

        // POST: api/Message
        [HttpPost]
        public Response<string> PostMessage(MessageReceiveDto messagedto)
        {
            //Create a new response with type string
            Response<string> response = new Response<string>();

            try {
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
