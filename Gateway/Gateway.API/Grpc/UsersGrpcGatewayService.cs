using Grpc.Core;
using Grpc.Net.Client;
using UsersRpc;

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
    }
}