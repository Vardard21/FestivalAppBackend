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

namespace FestivalApplication.Testing
{
    public class UserControllerPostTest : UserControllerTest
    {
        private const int UserPost_User3_UserID = 3;
        private const string UserPost_User3_UserName = "TestUsername3";
        private const string UserPost_User3_Password = "TestPassword3";
        private const string UserPost_User3_UserRole = "visitor";
        private const string UserPost_CreateNewUser_UserName = "NewUsername1";
        private const string UserPost_CreateNewUser_Password = "NewPassword1";

        public UserControllerPostTest()
    : base(
        new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase("Filename=UserPost.db")
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
                User user3 = new User();
                user3.UserID = UserPost_User3_UserID;
                user3.UserName = UserPost_User3_UserName;
                user3.PassWord = UserPost_User3_Password;
                user3.Role = UserPost_User3_UserRole;

                _context.User.AddRange(user3);

                _context.SaveChanges();
            }
        }


        [Fact]
        public void Fail_NoPassword()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                UserCreationDto dto = new UserCreationDto();
                dto.UserName = UserPost_CreateNewUser_UserName;

                //Act
                Response<int> response = controller.PostUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(4, response.ErrorMessage);
                Assert.IsType<int>(response.Data);
            }
        }

        [Fact]
        public void Fail_NoUserName()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                UserCreationDto dto = new UserCreationDto();
                dto.PassWord = UserPost_CreateNewUser_Password;

                //Act
                Response<int> response = controller.PostUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(4, response.ErrorMessage);
                Assert.IsType<int>(response.Data);
            }
        }

        [Fact]
        public void Fail_UserNameDuplicate()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                UserCreationDto dto = new UserCreationDto();
                dto.UserName = UserPost_User3_UserName;
                dto.PassWord = UserPost_CreateNewUser_Password;

                //Act
                Response<int> response = controller.PostUser(dto);

                //Assert
                Assert.False(response.Success);
                Assert.Contains(4, response.ErrorMessage);
                Assert.IsType<int>(response.Data);
            }
        }

        [Fact]
        public void Pass_OK()
        {
            using (DBContext _context = new DBContext(ContextOptions))
            {
                //Arrange
                UsersController controller = new UsersController(_context);
                UserCreationDto dto = new UserCreationDto();
                dto.UserName = UserPost_CreateNewUser_UserName;
                dto.PassWord = UserPost_CreateNewUser_Password;

                //Act
                Response<int> response = controller.PostUser(dto);

                //Assert
                Assert.True(response.Success);
                Assert.Empty(response.ErrorMessage);
                Assert.IsType<int>(response.Data);
                Assert.Equal("visitor", _context.User.Find(response.Data).Role);
            }
        }
    }
}
