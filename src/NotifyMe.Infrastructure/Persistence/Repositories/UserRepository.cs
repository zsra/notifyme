using Microsoft.EntityFrameworkCore;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Users;

namespace NotifyMe.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly NotifyMeDbContext _dbContext;

    public UserRepository(NotifyMeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
