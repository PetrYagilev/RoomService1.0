using RoomService.DTOs;

namespace RoomService.Services;
public interface IAuthService
{
    string GenerateJwtToken(Guid userId, string role);
    Task<TokenResponse> DummyLoginAsync(string role);
}

public interface IJwtService
{
    (Guid userId, string role) ValidateToken(string token);
}

