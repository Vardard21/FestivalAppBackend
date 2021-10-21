using FestivalApplication.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
