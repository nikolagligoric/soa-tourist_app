using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stakeholders.API.Controllers;
using Stakeholders.Application.DTOs;
using Stakeholders.Application.Interfaces;
using Stakeholders.Application.Services;
using Stakeholders.Domain.Entities;
using Stakeholders.Domain.Enums;
using Stakeholders.Infrastructure.Persistence;
using Stakeholders.Infrastructure.Repositories;
using Xunit;

namespace Stakeholders.API.Tests
{
    public class UserControllerIntegrationTests
    {
        [Fact]
        public void Register_ValidTourist_PersistsUserThroughRepositoryAndReturnsOk()
        {
            using var context = CreateDbContext();
            var controller = CreateController(context);

            var result = controller.Register(new RegistrationDto
            {
                FirstName = "Marko",
                LastName = "Sindjic",
                UserName = "marko",
                Password = "123",
                Email = "marko@example.com",
                Role = "Tourist"
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdUser = Assert.IsType<User>(okResult.Value);
            var savedUser = context.Users.Single(user => user.UserName == "marko");

            Assert.Equal(UserRole.Tourist, createdUser.Role);
            Assert.Equal("marko@example.com", savedUser.Email);
            Assert.False(savedUser.IsBlocked);
        }

        [Fact]
        public void Register_ExistingUsername_ReadsExistingUserFromDatabaseAndReturnsBadRequest()
        {
            using var context = CreateDbContext();
            context.Users.Add(CreateUser("marko"));
            context.SaveChanges();
            var controller = CreateController(context);

            var result = controller.Register(new RegistrationDto
            {
                FirstName = "Drugi",
                LastName = "Korisnik",
                UserName = "marko",
                Password = "456",
                Email = "drugi@example.com",
                Role = "Guide"
            });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User exists!", badRequest.Value);
            Assert.Equal(1, context.Users.Count(user => user.UserName == "marko"));
        }

        [Fact]
        public void Login_ValidCredentials_ReadsUserFromDatabaseAndReturnsMockedJwtToken()
        {
            using var context = CreateDbContext();
            var user = CreateUser("ana", password: "123", role: UserRole.Guide);
            context.Users.Add(user);
            context.SaveChanges();

            var jwtGeneratorMock = new Mock<IJwtGenerator>();
            jwtGeneratorMock.Setup(jwt => jwt.GenerateAccessToken(It.Is<User>(u => u.UserName == "ana")))
                            .Returns(new TokenDto { Id = user.Id, AccessToken = "jwt-token" });
            var controller = CreateController(context, jwtGeneratorMock.Object);

            var result = controller.Login(new LoginDto { Username = "ana", Password = "123" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("jwt-token", GetPropertyValue(okResult.Value!, "token"));
            jwtGeneratorMock.Verify(jwt => jwt.GenerateAccessToken(It.Is<User>(u => u.UserName == "ana")), Times.Once);
        }

        [Fact]
        public void GetAllUsers_MapsUsersLoadedFromDatabaseToResponseDtos()
        {
            using var context = CreateDbContext();
            context.Users.AddRange(
                CreateUser("admin", role: UserRole.Admin, isBlocked: false),
                CreateUser("tourist", role: UserRole.Tourist, isBlocked: true));
            context.SaveChanges();
            var controller = CreateController(context);

            var result = controller.GetAllUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsType<List<UserResponseDto>>(okResult.Value);

            Assert.Equal(2, users.Count);
            Assert.Contains(users, user => user.UserName == "admin" && user.Role == "Admin");
            Assert.Contains(users, user => user.UserName == "tourist" && user.IsBlocked);
        }

        [Fact]
        public void BlockUser_ValidTourist_UpdatesUserInDatabase()
        {
            using var context = CreateDbContext();
            var user = CreateUser("nemanja", role: UserRole.Tourist, isBlocked: false);
            context.Users.Add(user);
            context.SaveChanges();
            var controller = CreateController(context);

            var result = controller.BlockUser(user.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var savedUser = context.Users.Single(saved => saved.Id == user.Id);

            Assert.Equal("User blocked successfully!", GetPropertyValue(okResult.Value!, "message"));
            Assert.True(savedUser.IsBlocked);
        }

        [Fact]
        public void UpdateMyProfile_UsernameClaimIsUsedToUpdateDatabaseUser()
        {
            using var context = CreateDbContext();
            context.Users.Add(CreateUser("milica", firstName: "Milica", lastName: "Stara", role: UserRole.Guide));
            context.SaveChanges();
            var controller = CreateController(context);
            SetUserClaims(controller, "milica");

            var result = controller.UpdateMyProfile(new UpdateProfileDto
            {
                FirstName = "Milica",
                LastName = "Nova",
                ProfileImageUrl = "https://example.com/profile.png",
                Bio = "Vodi ture po planinama.",
                Motto = "Korak po korak."
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var savedUser = context.Users.Single(user => user.UserName == "milica");

            Assert.Equal("Profile updated successfully!", GetPropertyValue(okResult.Value!, "message"));
            Assert.Equal("Nova", savedUser.LastName);
            Assert.Equal("Vodi ture po planinama.", savedUser.Bio);
            Assert.Equal("Korak po korak.", savedUser.Motto);
        }

        private static UserController CreateController(AppDbContext context, IJwtGenerator? jwtGenerator = null)
        {
            var repository = new UserRepository(context);
            var userService = new UserService(repository, jwtGenerator ?? Mock.Of<IJwtGenerator>());
            return new UserController(userService);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static User CreateUser(
            string username,
            string password = "123",
            string firstName = "Test",
            string lastName = "User",
            string email = "test@example.com",
            UserRole role = UserRole.Tourist,
            bool isBlocked = false)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = username,
                Password = password,
                Email = email,
                Role = role,
                IsBlocked = isBlocked
            };
        }

        private static void SetUserClaims(UserController controller, string username)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("username", username)
            }, "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }

        private static object? GetPropertyValue(object value, string propertyName)
        {
            return value.GetType()
                        .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                        ?.GetValue(value);
        }
    }
}
