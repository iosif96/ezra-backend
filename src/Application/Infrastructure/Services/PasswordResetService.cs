using System.Net;

using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly ApplicationDbContext _context;
    private readonly IMailService _mailService;
    private readonly IDateTime _dateTime;
    private readonly ICryptographyService _cryptographyService;
    private readonly string _frontendUrl;

    private const int TokenExpirationHours = 24;

    public PasswordResetService(
        ApplicationDbContext context,
        IMailService mailService,
        IDateTime dateTime,
        ICryptographyService cryptographyService,
        IConfiguration configuration)
    {
        _context = context;
        _mailService = mailService;
        _dateTime = dateTime;
        _cryptographyService = cryptographyService;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Don't reveal whether a user exists
            return;
        }

        await SendPasswordResetEmailToUserAsync(user);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.IsUsed &&
                t.ExpiresAt > _dateTime.Now);

        return resetToken != null;
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.IsUsed &&
                t.ExpiresAt > _dateTime.Now);

        if (resetToken == null)
        {
            throw new InvalidOperationException("Invalid or expired password reset token.");
        }

        // Decrypt the password (sent encrypted from frontend)
        var decryptedPassword = _cryptographyService.Decrypt(newPassword);

        // Hash and save the new password
        resetToken.User.Password = decryptedPassword.Hash();
        resetToken.IsUsed = true;

        // Invalidate all existing sessions for security
        var userSessions = await _context.UserSessions
            .Where(s => s.UserId == resetToken.UserId)
            .ToListAsync();

        foreach (var session in userSessions)
        {
            session.RefreshToken = null;
            session.RefreshTokenExpiryDate = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SendPasswordResetEmailAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        await SendPasswordResetEmailToUserAsync(user);
    }

    private async Task SendPasswordResetEmailToUserAsync(User user)
    {
        // Invalidate any existing unused tokens
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();

        foreach (var existingToken in existingTokens)
        {
            existingToken.IsUsed = true;
        }

        // Generate new token
        var token = GenerateSecureToken();

        var passwordResetToken = new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = _dateTime.Now.AddHours(TokenExpirationHours),
            IsUsed = false,
            CreatedAt = _dateTime.Now
        };

        _context.PasswordResetTokens.Add(passwordResetToken);
        await _context.SaveChangesAsync();

        // Send email
        var resetLink = $"{_frontendUrl}/reset-password?token={token}";
        var emailBody = GeneratePasswordResetEmailBody(user.UserName ?? user.Email, resetLink);

        await _mailService.SendEmailAsync(
            user.Email,
            "Reset Your Password",
            emailBody);
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string GeneratePasswordResetEmailBody(string userName, string resetLink)
    {
        var encodedUserName = WebUtility.HtmlEncode(userName);
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>Password Reset Request</h2>
                <p>Hello {encodedUserName},</p>
                <p>We received a request to reset your password. Click the button below to create a new password:</p>
                <p style='margin: 30px 0;'>
                    <a href='{resetLink}'
                       style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                        Reset Password
                    </a>
                </p>
                <p>Or copy and paste this link into your browser:</p>
                <p style='word-break: break-all; color: #666;'>{resetLink}</p>
                <p><strong>This link will expire in 24 hours.</strong></p>
                <p>If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
                <p style='color: #999; font-size: 12px;'>Please do not reply to this email.</p>
            </div>
        ";
    }
}
