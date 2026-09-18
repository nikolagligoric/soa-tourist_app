using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Purchase.Application.Interfaces;
using Purchase.Application.Services;
using Purchase.Infrastructure.Clients;
using Purchase.Infrastructure.Database;
using Purchase.Infrastructure.Repositories;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Purchase.API.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols =
            Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });

    options.ListenAnyIP(8085, listenOptions =>
    {
        listenOptions.Protocols =
            Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddGrpc();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<PurchaseContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<ITourPurchaseTokenRepository, TourPurchaseTokenRepository>();

// Services
builder.Services.AddScoped<ShoppingCartService>();

builder.Services.AddScoped<ITourSlotReservationClient>(_ =>
    new TourSlotReservationClient(
        builder.Configuration["Nats:Url"] ?? "nats://nats:4222",
        builder.Configuration["Nats:TourSlotsCommandSubject"]
            ?? "tour.slots.command"));

// HTTP Clients
builder.Services.AddHttpClient<ITourClient, TourClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:Tours"]!);
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                ))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var instanceId =
    builder.Configuration["INSTANCE_ID"]
    ?? "unknown";

app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-ID";

    var correlationId = context.Request.Headers.TryGetValue(
        headerName,
        out var existingCorrelationId)
        ? existingCorrelationId.ToString()
        : string.Empty;

    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
    }

    context.Request.Headers[headerName] = correlationId;
    context.Response.Headers[headerName] = correlationId;
    context.Items["CorrelationId"] = correlationId;

    await next();
});

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Structured HTTP logging
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    var requestStartedAt = DateTime.UtcNow;

    Exception? caughtException = null;

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        caughtException = ex;
        throw;
    }
    finally
    {
        stopwatch.Stop();

        var statusCode = caughtException != null
            ? StatusCodes.Status500InternalServerError
            : context.Response.StatusCode;

        var level = caughtException != null
            ? "ERROR"
            : statusCode switch
            {
                >= 500 => "ERROR",
                >= 400 => "WARNING",
                _ => "INFO"
            };

        var logEntry = new
        {
            timestamp = requestStartedAt.ToString("O"),
            serviceName = "Purchase",
            instanceId,
            level,
            correlationId = context.Items["CorrelationId"]?.ToString(),
            method = context.Request.Method,
            path = context.Request.Path.Value,
            statusCode,
            latencyMs = Math.Round(
                stopwatch.Elapsed.TotalMilliseconds,
                3),
            message = "HTTP request",
            exception = caughtException == null
                ? null
                : $"{caughtException.GetType().Name}: {caughtException.Message}"
        };

        Console.WriteLine(
            JsonSerializer.Serialize(logEntry));
    }
});

// JWT
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGrpcService<PurchaseGrpcService>();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<PurchaseContext>();

    dbContext.Database.Migrate();
}

app.Run();