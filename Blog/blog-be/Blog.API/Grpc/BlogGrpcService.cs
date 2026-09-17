using Blog.Application.DTOs;
using Blog.Application.Services;
using BlogRpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Blog.API.Grpc;

public class BlogGrpcService : BlogRpcService.BlogRpcServiceBase
{
    private readonly BlogService _blogService;

    public BlogGrpcService(BlogService blogService)
    {
        _blogService = blogService;
    }

    public override async Task<AddCommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
    {
        var username = RequireHeader(context, "x-username");
        var token = RequireBearerToken(context);

        var dto = new CreateCommentDTO { Text = request.Comment?.Text ?? string.Empty };

        var comment = await _blogService.AddCommentAsync(request.BlogId, dto, username, token);

        return new AddCommentResponse
        {
            Comment = new BlogRpc.CommentDto
            {
                Id = comment.Id,
                AuthorUsername = comment.AuthorUsername,
                Text = comment.Text,
                CreatedAt = Timestamp.FromDateTime(ToUtc(comment.CreatedAt)),
                LastModifiedAt = Timestamp.FromDateTime(ToUtc(comment.LastModifiedAt)),
            }
        };
    }

    public override Task<LikeResponse> Like(LikeRequest request, ServerCallContext context)
    {
        var username = RequireHeader(context, "x-username");

        var likes = _blogService.LikeBlog(request.BlogId, username);
        return Task.FromResult(new LikeResponse { Likes = likes });
    }

    public override Task<LikeResponse> Unlike(UnlikeRequest request, ServerCallContext context)
    {
        var username = RequireHeader(context, "x-username");

        var likes = _blogService.UnlikeBlog(request.BlogId, username);
        return Task.FromResult(new LikeResponse { Likes = likes });
    }

    private static string RequireHeader(ServerCallContext context, string name)
    {
        var value = context.RequestHeaders.GetValue(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new RpcException(new Status(StatusCode.Unauthenticated, $"Missing metadata header '{name}'."));
        return value;
    }

    private static string RequireBearerToken(ServerCallContext context)
    {
        var auth = context.RequestHeaders.GetValue("authorization");
        if (string.IsNullOrWhiteSpace(auth))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing 'authorization' metadata."));

        const string bearer = "Bearer ";
        var token = auth.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
            ? auth.Substring(bearer.Length).Trim()
            : auth.Trim();

        if (string.IsNullOrWhiteSpace(token))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Empty token."));

        return token;
    }

    private static DateTime ToUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}