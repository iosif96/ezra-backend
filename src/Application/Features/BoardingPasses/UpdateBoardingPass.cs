using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.BoardingPasses.UpdateBoardingPass;

[Authorize]
public record UpdateBoardingPassCommand(int Id, string Code, string? Seat) : IRequest;

public class UpdateBoardingPassCommandValidator : AbstractValidator<UpdateBoardingPassCommand>
{
    public UpdateBoardingPassCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
    }
}

internal sealed class UpdateBoardingPassCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateBoardingPassCommand>
{
    public async Task Handle(UpdateBoardingPassCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.BoardingPasses
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(BoardingPass), request.Id);

        entity.Code = request.Code;
        entity.Seat = request.Seat;

        await context.SaveChangesAsync(cancellationToken);
    }
}
