using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Domain.Enums;
using Application.Features.Users.ChangeUserRole;
using Application.Features.Users.GetUserProfile;
using Application.Features.Users.GetUsersWithPagination;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

[ApiController]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IPasswordResetService _passwordResetService;

    public UsersController(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    [HttpGet]
    public Task<PaginatedList<UserBriefResponse>> GetUsersWithPagination([FromQuery] GetUsersWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{userId}/Role")]
    public async Task<ActionResult<ChangeUserRoleResponse>> ChangeUserRole(int userId, [FromBody] ChangeUserRoleRequest request)
    {
        var command = new ChangeUserRoleCommand(userId, request.Role);
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("{userId}")]
    public async Task<GetUserProfileResponse> GetUserProfile()
    {
        var command = new GetUserProfileCommand();
        return await Mediator.Send(command);
    }

    [HttpPost("{userId}/SendPasswordResetEmail")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendPasswordResetEmail(int userId)
    {
        try
        {
            await _passwordResetService.SendPasswordResetEmailAsync(userId);
            return Ok(new { message = "Password reset email sent successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while sending the password reset email.");
        }
    }
}

public record ChangeUserRoleRequest(Role Role);
