using Application.Common.Interfaces;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence.Configurations;

using Infrastructure.Persistence.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Application.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IDateTime _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTime dateTime)
        : base(options)
    {
        _dateTime = dateTime;
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<Gate> Gates => Set<Gate>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightMovement> FlightMovements => Set<FlightMovement>();
    public DbSet<BoardingPass> BoardingPasses => Set<BoardingPass>();
    public DbSet<Identity> Identities => Set<Identity>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Touchpoint> Touchpoints => Set<Touchpoint>();
    public DbSet<TouchpointScan> TouchpointScans => Set<TouchpointScan>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(token);
        try
        {
            await action();
            await SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoListConfiguration());
        modelBuilder.ApplyConfiguration(new ApiKeyConfiguration());

        modelBuilder.ApplySoftDeleteQueryFilter();

        base.OnModelCreating(modelBuilder);
    }
}