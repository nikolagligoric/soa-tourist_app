using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stakeholders.Application.DTOs;
using Stakeholders.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace Stakeholders.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegistrationDto registrationDto)
        {
            try
            {
                var user = _userService.RegisterUser(registrationDto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = _userService.Login(loginDto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/block")]
        public IActionResult BlockUser(int id)
        {
            try
            {
                _userService.BlockUser(id);
                return Ok(new { message = "User blocked successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Tourist,Guide")]
        [HttpGet("profile")]
        public IActionResult ViewMyProfile()
        {
            try
            {
                var username = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                if (username == null)
                {
                    return BadRequest("Invalid token!");
                }
                var userProfile = _userService.ViewMyProfile(username);
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
