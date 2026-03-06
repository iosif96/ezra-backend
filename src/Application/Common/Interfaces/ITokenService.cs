using System.Security.Claims;

using Application.Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal ValidateToken(string token);
    DateTime GetRefreshTokenExpiry();
}

