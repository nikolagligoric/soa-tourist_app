using Stakeholders.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Stakeholders.Application.DTOs;
using System.Security.Claims;
using Stakeholders.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Stakeholders.Infrastructure.Authentication
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly string _key = Environment.GetEnvironmentVariable("JWT_KEY") ?? "L1uKpZQzI1Yx0+OaS0kXkE7u0n/5Q0U3R5s3FVmXcXU=";
        private readonly string _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "stakeholder";
        private readonly string _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "stakeholder-front.com";

        public TokenDto GenerateAccessToken(User user)
        {
            var authenticationResponse = new TokenDto();

            var claims = new List<Claim>
            {
                new("username", user.UserName),
                new("role", user.Role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            authenticationResponse.AccessToken = tokenString;
            authenticationResponse.Id = user.Id;

            return authenticationResponse;
        }
    }
}
