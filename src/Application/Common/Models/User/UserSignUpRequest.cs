using System.ComponentModel;

namespace Application.Common.Models.User;

/// <summary>
/// Request for user sign up.
/// </summary>
public class UserSignUpRequest
{
    /// <summary>
    /// The email of the user.
    /// </summary>
    [DefaultValue("test@example.com")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The encrypted password of the user.
    /// </summary>
    [DefaultValue("JK7kXNqaG0VQJr2h2LnNRg==")]
    public string Password { get; set; } = string.Empty;
}
