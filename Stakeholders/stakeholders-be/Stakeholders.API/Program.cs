using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stakeholders.Application.Interfaces;
using Stakeholders.Application.Services;
using Stakeholders.Infrastructure.Authentication;
using Stakeholders.Infrastructure.Persistence;
using Stakeholders.Infrastructure.Repositories;
using Stakeholders.API.Grpc;
using System.Text;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddGrpc();
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
            new string[] {}
        }
    });
});


var serviceName = builder.Environment.ApplicationName;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddSource("Npgsql")
            .AddOtlpExporter(options =>
            {
                var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
                options.Endpoint = new Uri(endpoint);
            });
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();

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

var app = builder.Build();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding or migrating the database.");
    }
}

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
            serviceName = "Stakeholders",
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<UsersGrpcService>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();