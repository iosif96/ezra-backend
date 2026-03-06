using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Users.GetUsersWithPagination;

public record UserBriefResponse(int Id, string? UserName, string Email, Role Role);

[Authorize(Roles = "Admin")]
public record GetUsersWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<UserBriefResponse>>;

public class GetUsersWithPaginationQueryValidator : AbstractValidator<GetUsersWithPaginationQuery>
{
    public GetUsersWithPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}

internal sealed class GetUsersWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<UserBriefResponse>>
{
    private readonly ApplicationDbContext _context = context;

    public Task<PaginatedList<UserBriefResponse>> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return _context.Users
            .OrderBy(user => user.Email)
            .Select(user => ToDto(user))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }

    private static UserBriefResponse ToDto(User user)
    {
        return new(user.Id, user.UserName, user.Email, user.Role);
    }
}
