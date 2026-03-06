using Application.Domain.Enums;

namespace Application.Common.Models.User;

public class UserProfileResponse
{
    public string? UserName { get; set; }
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}
