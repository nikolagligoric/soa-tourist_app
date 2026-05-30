using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Purchase.Application.Interfaces;
using Purchase.Application.Services;
using Purchase.Infrastructure.Clients;
using Purchase.Infrastructure.Database;
using Purchase.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

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

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// JWT
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();