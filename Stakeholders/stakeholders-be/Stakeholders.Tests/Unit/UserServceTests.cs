using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Stakeholders.Application.DTOs;
using Stakeholders.Application.Interfaces;
using Stakeholders.Application.Services;
using Stakeholders.Domain.Entities;
using Stakeholders.Domain.Enums;

namespace Stakeholders.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtGenerator> _jwtGeneratorMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtGeneratorMock = new Mock<IJwtGenerator>();

            _userService = new UserService(_userRepositoryMock.Object, _jwtGeneratorMock.Object);
        }

        [Fact]
        public void RegisterUser_InvalidRole_ThrowsArgumentException()
        {
            
            var dto = new RegistrationDto { UserName = "marko", Role = "Admin" };

            
            var exception = Assert.Throws<ArgumentException>(() => _userService.RegisterUser(dto));
            Assert.Equal("UserRole must be 'Tourist' or 'Guide'!", exception.Message);

            
            _userRepositoryMock.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void RegisterUser_ExistingUsername_ThrowsArgumentException()
        {
            
            var dto = new RegistrationDto { UserName = "marko123", Role = "Tourist" };

            
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("marko123"))
                               .Returns(new User { UserName = "marko123" });

            
            var exception = Assert.Throws<ArgumentException>(() => _userService.RegisterUser(dto));
            Assert.Equal("User exists!", exception.Message);
        }

        [Fact]
        public void RegisterUser_ValidTouristData_ReturnsUserAndCallsRepository()
        {
            
            var dto = new RegistrationDto
            {
                FirstName = "Marko",
                LastName = "Sindjic",
                UserName = "marko",
                Password = "123",
                Email = "marko@example.com",
                Role = "Tourist"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("marko")).Returns((User)null);

            
            var result = _userService.RegisterUser(dto);

           
            Assert.NotNull(result);
            Assert.Equal(UserRole.Tourist, result.Role);
            Assert.False(result.IsBlocked);

            
            _userRepositoryMock.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void Login_BlockedUser_ReturnsBlockedString()
        {
           
            var loginDto = new LoginDto { Username = "blokirani", Password = "123" };
            var mockUser = new User { UserName = "blokirani", Password = "123", IsBlocked = true };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("blokirani")).Returns(mockUser);

           
            var result = _userService.Login(loginDto);

            
            Assert.Equal("BLOCKED", result);
            
            _jwtGeneratorMock.Verify(gen => gen.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void BlockUser_UserIsAdmin_ThrowsArgumentException()
        {
            
            int adminId = 1;
            var adminUser = new User { Id = adminId, UserName = "glavni_admin", Role = UserRole.Admin, IsBlocked = false };

            _userRepositoryMock.Setup(repo => repo.GetUserById(adminId)).Returns(adminUser);

            
            var exception = Assert.Throws<ArgumentException>(() => _userService.BlockUser(adminId));
            Assert.Equal("You can't block an admin!", exception.Message);
            Assert.False(adminUser.IsBlocked); 
        }

        [Fact]
        public void BlockUser_ValidUser_SetsIsBlockedToTrueAndUpdates()
        {
           
            int userId = 5;
            var targetUser = new User { Id = userId, UserName = "los_korisnik", Role = UserRole.Tourist, IsBlocked = false };

            _userRepositoryMock.Setup(repo => repo.GetUserById(userId)).Returns(targetUser);

            
            _userService.BlockUser(userId);

            
            Assert.True(targetUser.IsBlocked);
            
            _userRepositoryMock.Verify(repo => repo.UpdateUser(targetUser), Times.Once);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsAccessToken()
        {
            var loginDto = new LoginDto { Username = "marko", Password = "123" };
            var mockUser = new User { UserName = "marko", Password = "123", IsBlocked = false };
            var mockToken = new TokenDto { AccessToken = "validan_jwt_token_123" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("marko")).Returns(mockUser);

            _jwtGeneratorMock.Setup(gen => gen.GenerateAccessToken(mockUser)).Returns(mockToken);

            var result = _userService.Login(loginDto);

            Assert.NotNull(result);
            Assert.Equal("validan_jwt_token_123", result);

            _jwtGeneratorMock.Verify(gen => gen.GenerateAccessToken(mockUser), Times.Once);
        }

        [Fact]
        public void UpdateMyProfile_ValidData_UpdatesAllFieldsCorrectly()
        {
            var username = "nikola";
            var user = new User
            {
                UserName = username,
                FirstName = "StaroIme",
                LastName = "StaroPrezime",
                Bio = "Stara biografija",
                Motto = "Stari moto"
            };

            var dto = new UpdateProfileDto
            {
                FirstName = "Nikola",
                LastName = "Gligoric",
                Bio = "Nova biografija.",
                Motto = "Novi moto.",
                ProfileImageUrl = "nova_slika.jpg"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns(user);

            _userService.UpdateMyProfile(username, dto);

            Assert.Equal("Nikola", user.FirstName);
            Assert.Equal("Gligoric", user.LastName);
            Assert.Equal("Nova biografija.", user.Bio);
            Assert.Equal("Novi moto.", user.Motto);
            Assert.Equal("nova_slika.jpg", user.ProfileImageUrl);

            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.Is<User>(u =>
                u.FirstName == "Nikola" &&
                u.LastName == "Gligoric" &&
                u.Bio == "Nova biografija." &&
                u.Motto == "Novi moto.")), Times.Once);
        }

        [Fact]
        public void UpdateMyProfile_UserNotFound_ThrowsArgumentException()
        {
            var username = "nepostojeci";
            var dto = new UpdateProfileDto { FirstName = "Haker" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns((User)null);

            var exception = Assert.Throws<ArgumentException>(() => _userService.UpdateMyProfile(username, dto));

            Assert.Equal("User doesn't exists!", exception.Message);
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void GetProfile_ExistingUser_ReturnsUser()
        {
            var username = "nikola";
            var mockUser = new User
            {
                UserName = username,
                FirstName = "Nikola",
                LastName = "Gligoric",
                Role = UserRole.Tourist
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username))
                               .Returns(mockUser);

            var result = _userService.ViewMyProfile(username);

            Assert.NotNull(result);
            Assert.Equal("Nikola", result.FirstName);
            Assert.Equal("Gligoric", result.LastName);

            _userRepositoryMock.Verify(repo => repo.GetUserByUsername(username), Times.Once);
        }
    }
}