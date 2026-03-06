using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Domain.Entities;

public class UserSession
{
    [Key]
    public int Id { get; set; }
    public string DeviceInfo { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryDate { get; set; }
    public string? PushNotificationToken { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUsedDate { get; set; }

    public User User { get; set; } = null!;
}