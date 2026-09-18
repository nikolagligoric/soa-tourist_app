using Blog.Application.Interfaces;
using Blog.Infrastructure.Repositories;
using Blog.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MongoDB.Driver;
using Blog.API.Grpc;
using Blog.API.Messaging;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb"))
);

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("blog_db");
});
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddHostedService<TourPublishCommandSubscriber>();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();

builder.Services.AddGrpc();

var key = Encoding.UTF8.GetBytes("L1uKpZQzI1Yx0+OaS0kXkE7u0n/5Q0U3R5s3FVmXcXU=");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "stakeholder",
        ValidAudience = "stakeholder-front.com",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesi: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
            serviceName = "Blog",
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

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGrpcService<BlogGrpcService>();


app.Run();
