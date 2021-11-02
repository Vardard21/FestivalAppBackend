using FestivalApplication.Controllers;
using FestivalApplication.Data;
using FestivalApplication.Model;
using FestivalApplication.Model.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FestivalApplication.Testing
{
    public class UserControllerPutTest : UserControllerTest
    {
        private const int UserPut_User1_UserID = 1;
        private const string UserPut_User1_UserName = "TestUsername1";
        private const string UserPut_User1_Password = "TestPassword1";
        private const string UserPut_User1_UserRole = "visitor";
        private const string UserPut_User1_AuthenticationKey = "Auth1";
        private const int UserPut_Admin1_UserID = 2;
        private const string UserPut_Admin1_UserName = "TestUsername2";
        private const string UserPut_Admin1_Password = "TestPassword2";
        private const string UserPut_Admin1_UserRole = "admin";
        private const string UserPut_Admin1_AuthenticationKey = "Auth2";
        private readonly ITestOutputHelper output;

        public UserControllerPutTest(ITestOutputHelper output)
    : base(
        new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase("Filename=UserPut.db")
            .Options)
        {
            Seed();
            this.output = output;
        }

        private void Seed()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                //Enter Test Data
                User user1 = new User();
                user1.UserID = UserPut_User1_UserID;
                user1.UserName = UserPut_User1_UserName;
                user1.PassWord = UserPut_User1_Password;
                user1.Role = UserPut_User1_UserRole;

                Authentication auth1 = new Authentication();
                auth1.AuthenticationID = 1;
                auth1.AuthenticationKey = UserPut_User1_AuthenticationKey;
                auth1.User = user1;
                auth1.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth1.MaxExpiryDate = DateTime.UtcNow.AddHours(6);

                User admin1 = new User();
                admin1.UserID = UserPut_Admin1_UserID;
                admin1.UserName = UserPut_Admin1_UserName;
                admin1.PassWord = UserPut_Admin1_Password;
                admin1.Role = UserPut_Admin1_UserRole;

                Authentication auth2 = new Authentication();
                auth2.AuthenticationID = 2;
                auth2.AuthenticationKey = UserPut_Admin1_AuthenticationKey;
                auth2.User = admin1;
                auth2.CurrentExpiryDate = DateTime.UtcNow.AddMinutes(15);
                auth2.MaxExpiryDate = DateTime.UtcNow.AddHours(6);

                _context.User.AddRange(user1, admin1);
                _context.Authentication.AddRange(auth1, auth2);

                _context.SaveChanges();
            }
        }

        [Fact]
        public void Fail_IncorrectUserID()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_Admin1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserID = 0;
                dto.UserRole = "visitor";
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: " + UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_UserNotAdmin()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_User1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserID = UserPut_User1_UserID;
                dto.UserRole = "admin";
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: " + UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_NoUserID()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_Admin1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserRole = "visitor";
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: " + UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_NoRole()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_Admin1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserID = 0;
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: " + UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(3, response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }

        [Fact]
        public void Fail_RoleAlreadyAssigned()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_Admin1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserID = UserPut_User1_UserID;
                dto.UserRole = "visitor";
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: " + UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);

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
                controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = UserPut_Admin1_AuthenticationKey;
                ChangeRoleDto dto = new ChangeRoleDto();
                dto.UserID = UserPut_User1_UserID;
                dto.UserRole = "admin";
                output.WriteLine("Desired key: " + _context.Authentication.Find(2).AuthenticationKey);
                output.WriteLine("Used key: "+ UserPut_Admin1_AuthenticationKey);

                //Act
                Response<string> response = controller.PutUser(dto);
                //Response<string> response = controller.PutUser(dto);

                //Assert
                //Assert.True(response.Success);
                Assert.Empty(response.ErrorMessage);
                Assert.Null(response.Data);
            }
        }
    }
}
