using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Users;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();

    public void Seed(params User[] users)
    {
        foreach (var user in users)
        {
            _users[user.Id] = user;
        }
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(user => user.Email == email));

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }
}
