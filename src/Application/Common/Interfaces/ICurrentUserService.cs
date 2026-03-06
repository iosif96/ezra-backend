namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string DeviceInfo { get; }
    bool IsApiRequest { get; }
}