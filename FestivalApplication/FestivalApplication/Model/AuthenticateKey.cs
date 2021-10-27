using FestivalApplication.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FestivalApplication.Controllers;
using FestivalApplication.Model.DataTransferObjects;

namespace FestivalApplication.Model
{
    public class AuthenticateKey
    {
        private Boolean test = false;

        public Boolean Authenticate(DBContext context, string Key)
        {
            if (test)
            {
                return true;
            }
            else
            {
                var listofexpiredusers = context.Authentication.Where(x => x.CurrentExpiryDate < DateTime.UtcNow).Include(y=>y.User).ToList();
                foreach (Authentication authentication in listofexpiredusers)
                {
                    PutUserActivity(context, authentication.User.UserID);
                }

                context.Authentication.RemoveRange(context.Authentication.Where(x => x.CurrentExpiryDate < DateTime.UtcNow));
                context.SaveChanges();

                if (context.Authentication.Any(x => x.AuthenticationKey == Key))
                {
                    Authentication auth = context.Authentication.Where(x => x.AuthenticationKey == Key).FirstOrDefault();
                    if (auth.MaxExpiryDate < DateTime.UtcNow.AddMinutes(15))
                    {
                        auth.CurrentExpiryDate = auth.MaxExpiryDate;
                    }
                    else
                    {
                        auth.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                    }
                    context.Entry(auth).State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public Response<string> PutUserActivity(DBContext context,int UserID)
        {
            Response<string> response = new Response<string>();
            if (context.UserActivity.Where(x => x.User.UserID == UserID && x.Exit == default).Any()) 
            {             
                
                var ActiveActivity = context.UserActivity.Where(x => x.User.UserID == UserID && x.Exit == default).First();
                ActiveActivity.Exit = DateTime.UtcNow;
                context.Entry(ActiveActivity).State = EntityState.Modified;
                if (context.SaveChanges() > 0)
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
            }else
            {
                response.Success = true;
                return response;
            }

        }

    }

}
