using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FestivalApplication.Model;

namespace FestivalApplication.Data
{
    public class DBContext : DbContext
    {
        public DBContext (DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<FestivalApplication.Model.Message> Message { get; set; }
        public DbSet<FestivalApplication.Model.UserActivity> UserActivity { get; set; }
        public DbSet<FestivalApplication.Model.User> User { get; set; }
        public DbSet<FestivalApplication.Model.Stage> Stage { get; set; }
        public DbSet<FestivalApplication.Model.Authentication> Authentication { get; set; }
    }
}
