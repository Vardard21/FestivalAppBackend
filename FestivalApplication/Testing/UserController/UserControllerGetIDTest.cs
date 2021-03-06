using FestivalApplication.Controllers;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FestivalApplication.Testing
{
    public class UserControllerGetIDTest : UserControllerTest
    {
        private User user1;
        private User admin1;
        private Authentication auth1;
        private Authentication auth2;
        private Stage stage1;
        private UserActivity activity1;
        private UserActivity activity2;
        private Message message1;
        private Message message2;

        public UserControllerGetIDTest()
    : base(
        new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase("Filename=UserGetID.db")
            .Options)
        {
            Seed();
        }

        private void Seed()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();
                //Enter Test Data
                user1 = new User();
                user1.UserID = 1;
                user1.UserName = "UserName1";
                user1.PassWord = "Password1";
                user1.Role = "visitor";
                admin1 = new User();
                admin1.UserID = 2;
                admin1.UserName = "UserName2";
                admin1.PassWord = "Password2";
                admin1.Role = "admin";
                auth1 = new Authentication();
                auth1.AuthenticationID = 1;
                auth1.AuthenticationKey = "Auth1";
                auth1.User = admin1;
                auth1.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth1.MaxExpiryDate = DateTime.UtcNow.AddHours(6);
                auth2 = new Authentication();
                auth2.AuthenticationID = 2;
                auth2.AuthenticationKey = "Auth2";
                auth2.User = user1;
                auth2.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth2.MaxExpiryDate = DateTime.UtcNow.AddHours(6);
                stage1 = new Stage();
                stage1.StageID = 1;
                stage1.StageName = "Stage1";
                stage1.StageActive = true;
                stage1.Archived = false;
                activity1 = new UserActivity();
                activity1.UserActivityID = 1;
                activity1.User = user1;
                activity1.Stage = stage1;
                activity1.Entry = DateTime.UtcNow.AddMinutes(-60);
                activity1.Exit = DateTime.UtcNow.AddMinutes(-20);
                activity2 = new UserActivity();
                activity2.UserActivityID = 2;
                activity2.User = user1;
                activity2.Stage = stage1;
                activity2.Entry = DateTime.UtcNow.AddMinutes(-5);
                activity2.Exit = default;
                message1 = new Message();
                message1.MessageID = 1;
                message1.MessageText = "MessageText1";
                message1.Timestamp = DateTime.UtcNow.AddMinutes(-30);
                message1.UserActivity = activity1;
                message2 = new Message();
                message2.MessageID = 2;
                message2.MessageText = "MessageText2";
                message2.Timestamp = DateTime.UtcNow.AddMinutes(-28);
                message2.UserActivity = activity1;

                _context.AddRange(user1, admin1, activity1, activity2, stage1, message1, message2, auth1, auth2);
                _context.SaveChanges();
            }
        }


        [Fact]
        public void Fail_NoAuthKey()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();

                //Act
                Response<UserDetailsDto> response = controller.GetUser(user1.UserID);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(5, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_AuthKeyNotFromAdmin()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = auth2.AuthenticationKey;

                //Act
                Response<UserDetailsDto> response = controller.GetUser(user1.UserID);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_UserDoesNotExist()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = auth1.AuthenticationKey;

                //Act
                Response<UserDetailsDto> response = controller.GetUser(8);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Pass_OK()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = auth1.AuthenticationKey;

                //Act
                Response<UserDetailsDto> response = controller.GetUser(user1.UserID);

                //Assert
                Assert.True(response.Success);
                Assert.Empty(response.ErrorMessage);
                Assert.IsType<UserDetailsDto>(response.Data);
                Assert.Equal(user1.UserID, response.Data.UserID);
                Assert.Equal(user1.UserName, response.Data.UserName);
                Assert.Equal(user1.Role, response.Data.UserRole);
                Assert.Equal(2, response.Data.activities.Count());
                Assert.Equal(2, response.Data.activities[0].MessageHistory.Count());
            }
        }
    }
}
