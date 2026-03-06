using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.User;

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
