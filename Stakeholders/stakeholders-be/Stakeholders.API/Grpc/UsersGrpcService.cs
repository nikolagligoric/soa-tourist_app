using Grpc.Core;
using Stakeholders.Application.DTOs;
using Stakeholders.Application.Services;
using UsersRpc;

namespace Stakeholders.API.Grpc
{
    public class UsersGrpcService : UsersRpcService.UsersRpcServiceBase
    {
        private readonly UserService _userService;

        public UsersGrpcService(UserService userService)
        {
            _userService = userService;
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var dto = new RegistrationDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Password = request.Password,
                Email = request.Email,
                Role = request.Role
            };

            var user = _userService.RegisterUser(dto);

            return Task.FromResult(new RegisterResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var dto = new LoginDto
            {
                Username = request.Username,
                Password = request.Password
            };

            var token = _userService.Login(dto);

            return Task.FromResult(new LoginResponse
            {
                Token = token
            });
        }

        public override Task<ProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
        {
            var profile = _userService.ViewMyProfile(request.Username);

            return Task.FromResult(new ProfileResponse
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                ProfileImageUrl = profile.ProfileImageUrl ?? "",
                Bio = profile.Bio ?? "",
                Motto = profile.Motto ?? ""
            });
        }

        public override Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
        {
            var dto = new UpdateProfileDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfileImageUrl = request.ProfileImageUrl,
                Bio = request.Bio,
                Motto = request.Motto
            };

            _userService.UpdateMyProfile(request.Username, dto);

            return Task.FromResult(new UpdateProfileResponse
            {
                Message = "Profile updated successfully!"
            });
        }
    }
}