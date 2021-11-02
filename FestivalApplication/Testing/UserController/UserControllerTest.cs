using FestivalApplication.Controllers;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FestivalApplication.Testing
{
    public class UserControllerTest
    {
        //Seed the database on launching the test for the users controller
        protected UserControllerTest(DbContextOptions<DBContext> contextOptions)
        {
            ContextOptions = contextOptions;
        }

        protected DbContextOptions<DBContext> ContextOptions { get; }
    }
}
