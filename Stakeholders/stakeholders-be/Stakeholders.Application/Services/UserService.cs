using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stakeholders.Application.DTOs;
using Stakeholders.Application.Interfaces;
using Stakeholders.Domain.Entities;
using Stakeholders.Domain.Enums;

namespace Stakeholders.Application.Services
{
    public class UserService
    {
        private IUserRepository _userRepository;
        private IJwtGenerator _jwtGenerator;
        public UserService(IUserRepository userRepository, IJwtGenerator jwtGenerator)
        {
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
        }
        public User RegisterUser(RegistrationDto registrationDto)
        {
            if (registrationDto.Role != "Guide" && registrationDto.Role != "Tourist")
            {
                throw new ArgumentException("UserRole must be 'Tourist' or 'Guide'!");
            }

            if (_userRepository.GetUserByUsername(registrationDto.UserName) != null)
            {
                throw new ArgumentException("User exists!");
            }

            UserRole role;

            if (registrationDto.Role == "Guide")
            {
                role = UserRole.Guide;
            }
            else
            {
                role = UserRole.Tourist;
            }

            User user = new User
            {
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                UserName = registrationDto.UserName,
                Password = registrationDto.Password,
                Email = registrationDto.Email,
                Role = role,
                IsBlocked = false

            };

            _userRepository.AddUser(user);

            return user;
        }

        public string Login(LoginDto loginDto)
        {
            User user = _userRepository.GetUserByUsername(loginDto.Username);

            if (user == null)
            {
                throw new ArgumentException("User doesn't exists!");
            }

            if (loginDto.Password != user.Password)
            {
                throw new ArgumentException("Password is incorrect!");
            }

            var token = _jwtGenerator.GenerateAccessToken(user);
            return token.AccessToken;
        }

        public List<UserResponseDto> GetAllUsers()
        {
            var users = _userRepository.GetAllUsers();

            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsBlocked = u.IsBlocked
            }).ToList();
        }

        public void BlockUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                throw new ArgumentException("User doesn't exists!");
            }
            if(user.Role == UserRole.Admin)
            {
                throw new ArgumentException("You can't block an admin!");
            }
            if(user.IsBlocked)
            {
                throw new ArgumentException("User is already blocked!");
            }
            user.IsBlocked = true;
            _userRepository.UpdateUser(user);
        }
    }
}
