using Stakeholders.Domain.Entities;
using Stakeholders.Application.DTOs;

namespace Stakeholders.Application.Interfaces
{
    public interface IJwtGenerator
    {
        TokenDto GenerateAccessToken(User user);
    }
}
