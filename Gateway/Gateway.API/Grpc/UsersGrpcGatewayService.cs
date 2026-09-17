using Grpc.Core;
using Grpc.Net.Client;
using UsersRpc;
using System.IdentityModel.Tokens.Jwt;

namespace Gateway.API.Grpc
{
    public class UsersGrpcGatewayService : UsersRpcService.UsersRpcServiceBase
    {
        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(
                "https://stakeholders:8080",
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );

            var client = new UsersRpcService.UsersRpcServiceClient(channel);

            return await client.RegisterAsync(request);
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(
                "https://stakeholders:8080",
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );

            var client = new UsersRpcService.UsersRpcServiceClient(channel);

            return await client.LoginAsync(request);
        }

        public override async Task<ProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
        {
            var username = ExtractUsername(context);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(
                "https://stakeholders:8080",
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );

            var client = new UsersRpcService.UsersRpcServiceClient(channel);

            return await client.GetProfileAsync(new GetProfileRequest
            {
                Username = username
            });
        }

        public override async Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
        {
            var username = ExtractUsername(context);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(
                "https://stakeholders:8080",
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );

            var client = new UsersRpcService.UsersRpcServiceClient(channel);

            return await client.UpdateProfileAsync(new UpdateProfileRequest
            {
                Username = username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfileImageUrl = request.ProfileImageUrl,
                Bio = request.Bio,
                Motto = request.Motto
            });
        }

        private static string ExtractUsername(ServerCallContext context)
        {
            var auth = context.RequestHeaders.GetValue("authorization");

            if (string.IsNullOrWhiteSpace(auth))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing Authorization header."));

            const string bearer = "Bearer ";

            var token = auth.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
                ? auth.Substring(bearer.Length).Trim()
                : auth.Trim();

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

            if (string.IsNullOrWhiteSpace(username))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Username claim not found in token."));

            return username;
        }

    }
}