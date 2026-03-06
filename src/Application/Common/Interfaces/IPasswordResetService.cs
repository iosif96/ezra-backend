namespace Application.Common.Interfaces;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(string email);
    Task<bool> ValidateTokenAsync(string token);
    Task ResetPasswordAsync(string token, string newPassword);
    Task SendPasswordResetEmailAsync(int userId);
}
