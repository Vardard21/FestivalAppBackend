using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FestivalApplication.Controllers
{
    [Route("api/Login")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DBContext _context;

        public AuthenticationController(DBContext context)
        {
            _context = context;
        }

        // POST api/<LoginController>
        [HttpPost]
        public Response<UserLoginDto> Post(UserCreationDto logindetails)
        {
            //Create a new response to be send out
            Response<UserLoginDto> response = new Response<UserLoginDto>();

            try
            {
                //Check if UserName & PassWord combination is valid
                if (_context.User.Any(x => x.UserName == logindetails.UserName && x.PassWord == logindetails.PassWord))
                {
                    //Get User Information details
                    User user = _context.User.Where(x => x.UserName == logindetails.UserName).FirstOrDefault();

                    //Check for existing AuthenticationKeys
                    if (_context.Authentication.Any(x => x.User.UserID == user.UserID))
                    {
                        //Delete Existing Keys for this user
                        _context.Authentication.Remove(_context.Authentication.Where(x => x.User.UserID == user.UserID).FirstOrDefault());
                    }

                    //Generate a new entry for the authentication table
                    Authentication auth = new Authentication();
                    auth.User = user;
                    auth.AuthenticationKey = GenerateAuthenticationKey();
                    auth.MaxExpiryDate = DateTime.UtcNow.AddHours(6);
                    auth.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(30);

                    //Save the auth object
                    _context.Authentication.Add(auth);
                    if (_context.SaveChanges() > 0)
                    {
                        //Authkey was saved correctly
                        response.Success = true;

                        //Generate and populate the dto
                        UserLoginDto dto = new UserLoginDto();
                        dto.UserID = user.UserID;
                        dto.UserName = user.UserName;
                        dto.UserRole = user.Role;
                        dto.AuthenticationKey = auth.AuthenticationKey;
                        response.Data = dto;
                        return response;
                    }
                    else
                    {
                        //Authkey was not saved correctly
                        response.Success = false;
                        response.ErrorMessage.Add(1);
                        return response;
                    }
                }
                else
                {
                    //Combination is incorrect
                    response.Success = false;
                    response.ErrorMessage.Add(4);
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

        // DELETE api/<LoginController>/5
        [HttpDelete("{userid}")]
        public Response<string> Delete(int userid)
        {
            //Create a new response
            Response<string> response = new Response<string>();

            try
            {
                //Check for existing authkeys
                if (_context.Authentication.Any(x => x.User.UserID == userid))
                {
                    //Delete the key
                    _context.Authentication.Remove(_context.Authentication.Where(x => x.User.UserID == userid).FirstOrDefault());

                    //Save changes
                    if (_context.SaveChanges() > 0)
                    {
                        //Key was removed correctly
                        response.Success = true;
                        return response;
                    }
                    else
                    {
                        //Key was not removed correctly
                        response.Success = false;
                        response.ErrorMessage.Add(1);
                        return response;
                    }
                } else
                {
                    //No keys were found
                    response.Success = false;
                    response.ErrorMessage.Add(3);
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

        private string GenerateAuthenticationKey()
        {
            int size = 25;
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }
            return result.ToString();
        }

        //AuthenticateKey auth = new AuthenticateKey();
        //        if (!auth.Authenticate(_context, Request.Headers["Authorization"]))
        //        {
        //        }
        //        else
        //        {
        //            response.Success = false;
        //            response.ErrorMessage.Add(5);
        //            return response;
        //        }

}
}
