using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Grpc.Net.Client;
using BlogRpc;

namespace Gateway.API.Grpc
{
    public class BlogGrpcGatewayService : BlogRpcService.BlogRpcServiceBase
    {
        private const string BlogServiceAddress = "https://blog:8080";

        public override async Task<AddCommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
        {
            var (authHeader, username) = ExtractAuthAndUsername(context);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var channel = GrpcChannel.ForAddress(
                BlogServiceAddress,
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );
            var client = new BlogRpcService.BlogRpcServiceClient(channel);

            var headers = new Metadata
            {
                { "authorization", authHeader },
                { "x-username", username }
            };

            return await client.AddCommentAsync(request, headers);
        }

        public override async Task<LikeResponse> Like(LikeRequest request, ServerCallContext context)
        {
            var (authHeader, username) = ExtractAuthAndUsername(context);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var channel = GrpcChannel.ForAddress(
                BlogServiceAddress,
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );
            var client = new BlogRpcService.BlogRpcServiceClient(channel);

            var headers = new Metadata
            {
                { "authorization", authHeader },
                { "x-username", username }
            };

            return await client.LikeAsync(request, headers);
        }

        public override async Task<LikeResponse> Unlike(UnlikeRequest request, ServerCallContext context)
        {
            var (authHeader, username) = ExtractAuthAndUsername(context);

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var channel = GrpcChannel.ForAddress(
                BlogServiceAddress,
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );
            var client = new BlogRpcService.BlogRpcServiceClient(channel);

            var headers = new Metadata
            {
                { "authorization", authHeader },
                { "x-username", username }
            };

            return await client.UnlikeAsync(request, headers);
        }

        private static (string AuthorizationHeader, string Username) ExtractAuthAndUsername(ServerCallContext context)
        {
            var auth = context.RequestHeaders.GetValue("authorization");
            if (string.IsNullOrWhiteSpace(auth))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing Authorization header."));

            const string bearer = "Bearer ";
            var token = auth.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
                ? auth.Substring(bearer.Length).Trim()
                : auth.Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Empty token."));

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid JWT token format."));
            }

            var username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            if (string.IsNullOrWhiteSpace(username))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Username claim not found in token."));

            var authHeader = auth.StartsWith(bearer, StringComparison.OrdinalIgnoreCase) ? auth : $"{bearer}{token}";

            return (authHeader, username);
        }
    }
}