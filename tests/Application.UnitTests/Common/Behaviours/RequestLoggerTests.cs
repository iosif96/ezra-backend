using Application.Common.Behaviours;
using Application.Common.Interfaces;

using Microsoft.Extensions.Logging;

using Moq;

using CreateTodoItem = Application.Features.TodoItems.CreateTodoItem;

namespace Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private readonly Mock<ILogger<CreateTodoItem.CreateTodoItemCommand>> _logger;
    private readonly Mock<ICurrentUserService> _currentUserService;

    public RequestLoggerTests()
    {
        _logger = new Mock<ILogger<CreateTodoItem.CreateTodoItemCommand>>();
        _currentUserService = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _currentUserService.Setup(x => x.UserId).Returns(0);

        var requestLogger = new LoggingBehaviour<CreateTodoItem.CreateTodoItemCommand>(_logger.Object, _currentUserService.Object);

        await requestLogger.Process(new CreateTodoItem.CreateTodoItemCommand(1, "title"), CancellationToken.None);
    }

    [Fact]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateTodoItem.CreateTodoItemCommand>(_logger.Object, _currentUserService.Object);

        await requestLogger.Process(new CreateTodoItem.CreateTodoItemCommand(1, "title"), CancellationToken.None);
    }
}