using Gateway.API.Grpc;
using TourExecutionRpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddGrpc().AddJsonTranscoding();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapGrpcService<UsersGrpcGatewayService>();
app.MapGrpcService<BlogGrpcGatewayService>();
app.MapGrpcService<TourExecutionGrpcGatewayService>();

app.MapControllers();
async Task ProxyRequest(HttpContext context, string targetBaseUrl, string path)
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    var httpClient = new HttpClient(handler);

    var targetUri = targetBaseUrl + "/" + path + context.Request.QueryString;

    using var requestMessage = new HttpRequestMessage
    {
        Method = new HttpMethod(context.Request.Method),
        RequestUri = new Uri(targetUri)
    };

    if (context.Request.ContentLength > 0)
    {
        requestMessage.Content = new StreamContent(context.Request.Body);

        if (!string.IsNullOrEmpty(context.Request.ContentType))
        {
            requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
        }
    }

    foreach (var header in context.Request.Headers)
    {
        if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            continue;

        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
    }

    var responseMessage = await httpClient.SendAsync(requestMessage);

    context.Response.StatusCode = (int)responseMessage.StatusCode;

    foreach (var header in responseMessage.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    foreach (var header in responseMessage.Content.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    context.Response.Headers.Remove("transfer-encoding");

    await responseMessage.Content.CopyToAsync(context.Response.Body);
}

app.MapPost("/api/tour-executions/{tourId}/start-rpc", async (
    long tourId,
    HttpContext context) =>
{
    var authHeader = context.Request.Headers.Authorization.ToString();

    if (string.IsNullOrWhiteSpace(authHeader))
        return Results.Unauthorized();

    var token = authHeader.StartsWith("Bearer ")
        ? authHeader.Substring("Bearer ".Length)
        : authHeader;

    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);

    var username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
    var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

    using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("http://tours:9091");
    var client = new TourExecutionRpcService.TourExecutionRpcServiceClient(channel);

    var response = await client.StartTourAsync(new StartTourRequest
    {
        TourId = tourId,
        Username = username ?? "",
        Role = role ?? "",
        Token = token
    });

    return Results.Ok(response);
});

app.Map("/{**path}", async (HttpContext context, string path) =>
{
    if (path.StartsWith("stakeholders/", StringComparison.OrdinalIgnoreCase))
    {
        var newPath = path["stakeholders/".Length..];
        await ProxyRequest(context, "https://stakeholders:8080", newPath);
        return;
    }

    if (path.StartsWith("blog/", StringComparison.OrdinalIgnoreCase))
    {
        var newPath = path["blog/".Length..];
        await ProxyRequest(context, "https://blog:8080", newPath);
        return;
    }

    if (path.StartsWith("followers/", StringComparison.OrdinalIgnoreCase))
    {
        var newPath = path["followers/".Length..];
        await ProxyRequest(context, "http://followers:8082", newPath);
        return;
    }

    if (path.StartsWith("tours/", StringComparison.OrdinalIgnoreCase))
    {
        var newPath = path["tours/".Length..];
        await ProxyRequest(context, "http://tours:8083", newPath);
        return;
    }

    if (path.StartsWith("purchase/", StringComparison.OrdinalIgnoreCase))
    {
        var newPath = path["purchase/".Length..];
        await ProxyRequest(context, "http://purchase:8080", newPath);
        return;
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsync("Gateway route not found.");
});
app.Run();