using Application.Common.Configuration;
using Application.Common.Interfaces;
using Application.Common.Models.User;
using Application.Common.Utilities;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly TokenConfiguration _configuration;
    private readonly IDateTime _dateTime;
    private readonly ICryptographyService _cryptographyService;
    public AuthService(
        ApplicationDbContext context,
        IMapper mapper,
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        IDateTime dateTime,
        ICryptographyService cryptographyService)
    {
        _context = context;
        _mapper = mapper;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _configuration = TokenConfiguration.FromConfiguration(configuration);
        _dateTime = dateTime;
        _cryptographyService = cryptographyService;
    }

    public async Task<UserSignInResponse> SignIn(UserSignInRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(x => x.Email == request.Email)
            ?? throw new UserLoginFailedException("Incorrect email or password.");

        var decryptedPassword = _cryptographyService.Decrypt(request.Password);

        if (!PasswordEncoder.Verify(decryptedPassword, user.Password))
        {
            throw new UserLoginFailedException("Incorrect email or password.");
        }

        var accessToken = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var deviceInfo = _currentUserService.DeviceInfo;

        // Try to find existing device
        var currentSession = user.Sessions.FirstOrDefault(d => d.DeviceInfo == deviceInfo);

        if (currentSession == null)
        {
            // Create new refresh token entry
            var userRefreshToken = new UserSession
            {
                DeviceInfo = deviceInfo,
                UserId = user.Id,
                RefreshToken = refreshToken,
                CreatedDate = _dateTime.Now,
                RefreshTokenExpiryDate = _dateTime.Now.Add(_configuration.RefreshTokenLifetime),
                LastUsedDate = _dateTime.Now,
            };

            user.Sessions.Add(userRefreshToken);
        }
        else
        {
            // Update existing device
            currentSession.RefreshToken = refreshToken;
            currentSession.LastUsedDate = _dateTime.Now;
            currentSession.RefreshTokenExpiryDate = _dateTime.Now.Add(_configuration.RefreshTokenLifetime);
            currentSession.LastUsedDate = _dateTime.Now;
        }

        await _context.SaveChangesAsync();

        var response = _mapper.Map<UserSignInResponse>(user);
        response.Token = accessToken;
        response.RefreshToken = refreshToken;
        return response;
    }

    public async Task<UserSignUpResponse> SignUp(UserSignUpRequest request)
    {
        var isEmailExist = await _context.Users.AnyAsync(x => x.Email == request.Email);
        if (isEmailExist)
        {
            throw new UserRegisterFailedException($"User with email {request.Email} already exists.");
        }

        var decryptedPassword = _cryptographyService.Decrypt(request.Password);

        var user = _mapper.Map<User>(request);
        var deviceInfo = _currentUserService.DeviceInfo;
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.Password = decryptedPassword.Hash();
        user.Sessions.Add(new UserSession
        {
            DeviceInfo = deviceInfo,
            RefreshToken = refreshToken,
            CreatedDate = _dateTime.Now,
            RefreshTokenExpiryDate = _dateTime.Now.Add(_configuration.RefreshTokenLifetime),
        });
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateToken(user);

        var response = _mapper.Map<UserSignUpResponse>(user);
        response.Token = accessToken;
        response.RefreshToken = refreshToken;
        return response;
    }

    public async Task Logout(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var user = await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Sessions.Any(s => s.RefreshToken == refreshToken));

        if (user == null)
        {
            return;
        }

        var currentSession = user.Sessions.FirstOrDefault(s => s.RefreshToken == refreshToken);
        if (currentSession != null)
        {
            currentSession.RefreshToken = null;
            currentSession.RefreshTokenExpiryDate = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<RefreshTokenResponse> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token not found");
        }

        var user = await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Sessions.Any(t =>
                t.RefreshToken == refreshToken &&
                t.RefreshTokenExpiryDate > _dateTime.Now));

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var currentSession = user.Sessions.First(t => t.RefreshToken == refreshToken);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Update existing token
        currentSession.RefreshToken = newRefreshToken;
        currentSession.RefreshTokenExpiryDate = _dateTime.Now.Add(_configuration.RefreshTokenLifetime);
        currentSession.LastUsedDate = _dateTime.Now;
        await _context.SaveChangesAsync();

        return new RefreshTokenResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<UserProfileResponse> GetProfile()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return _mapper.Map<UserProfileResponse>(user);
    }

    public async Task<UserProfileResponse> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        user.UserName = request.UserName;
        await _context.SaveChangesAsync();

        return _mapper.Map<UserProfileResponse>(user);
    }
}