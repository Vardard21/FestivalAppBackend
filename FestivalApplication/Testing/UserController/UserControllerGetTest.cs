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
    public class UserControllerGetTest : UserControllerTest
    {
        private const int UserGet_User1_UserID = 1;
        private const string UserGet_User1_UserName = "TestUsername1";
        private const string UserGet_User1_Password = "TestPassword1";
        private const string UserGet_User1_UserRole = "visitor";
        private const string UserGet_User1_AuthenticationKey = "Auth2";
        private const int UserGet_User2_UserID = 2;
        private const string UserGet_User2_UserName = "TestUsername2";
        private const string UserGet_User2_Password = "TestPassword2";
        private const string UserGet_User2_UserRole = "visitor";
        private const int UserGet_Admin1_UserID = 3;
        private const string UserGet_Admin1_UserName = "TestUsername3";
        private const string UserGet_Admin1_Password = "TestPassword3";
        private const string UserGet_Admin1_UserRole = "admin";
        private const string UserGet_Admin1_AuthenticationKey = "Auth1";

        public UserControllerGetTest()
    : base(
        new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase("Filename=UserGet.db")
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
                User user1 = new User();
                user1.UserID = UserGet_User1_UserID;
                user1.UserName = UserGet_User1_UserName;
                user1.PassWord = UserGet_User1_Password;
                user1.Role = UserGet_User1_UserRole;
                User user2 = new User();
                user2.UserID = UserGet_User2_UserID;
                user2.UserName = UserGet_User2_UserName;
                user2.PassWord = UserGet_User2_Password;
                user2.Role = UserGet_User2_UserRole;
                User admin1 = new User();
                admin1.UserID = UserGet_Admin1_UserID;
                admin1.UserName = UserGet_Admin1_UserName;
                admin1.PassWord = UserGet_Admin1_Password;
                admin1.Role = UserGet_Admin1_UserRole;

                Authentication auth1 = new Authentication();
                auth1.AuthenticationID = 1;
                auth1.AuthenticationKey = UserGet_Admin1_AuthenticationKey;
                auth1.User = admin1;
                auth1.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth1.MaxExpiryDate = DateTime.UtcNow.AddHours(6);
                Authentication auth2 = new Authentication();
                auth2.AuthenticationID = 2;
                auth2.AuthenticationKey = UserGet_User1_AuthenticationKey;
                auth2.User = user1;
                auth2.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth2.MaxExpiryDate = DateTime.UtcNow.AddHours(6);

                _context.User.AddRange(user1, user2, admin1);
                _context.Authentication.AddRange(auth1, auth2);

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
                Response<List<UserSendDto>> response = controller.GetUser();

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
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserGet_User1_AuthenticationKey;

                //Act
                Response<List<UserSendDto>> response = controller.GetUser();

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
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserGet_Admin1_AuthenticationKey;

                //Act
                Response<List<UserSendDto>> response = controller.GetUser();

                //Assert
                Assert.True(response.Success);
                Assert.Empty(response.ErrorMessage);
                Assert.IsType<List<UserSendDto>>(response.Data);
                Assert.Equal(3, response.Data.Count());
            }
        }
    }
}
