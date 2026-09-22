using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Application.Users;
using NotifyMe.Domain.Users;
using Xunit;

namespace NotifyMe.Application.Tests.Users;

public class LoginUserUseCaseTests
{
    private static LoginUserUseCase CreateUseCase(out InMemoryUserRepository repository)
    {
        repository = new InMemoryUserRepository();
        return new LoginUserUseCase(
            repository, new FakePasswordHasher(), new FakeJwtTokenGenerator(), new LoginUserRequestValidator());
    }

    [Fact]
    public async Task ExecuteAsync_WithCorrectCredentials_ReturnsToken()
    {
        var useCase = CreateUseCase(out var repository);
        var hasher = new FakePasswordHasher();
        var user = User.Create(Guid.NewGuid(), "someone@example.com", hasher.HashPassword("correct-password"), DateTimeOffset.UtcNow);
        repository.Seed(user);

        var result = await useCase.ExecuteAsync(new LoginUserRequest("someone@example.com", "correct-password"), CancellationToken.None);

        Assert.Equal(user.Id, result.User.Id);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongPassword_ThrowsAuthenticationFailedException()
    {
        var useCase = CreateUseCase(out var repository);
        var hasher = new FakePasswordHasher();
        var user = User.Create(Guid.NewGuid(), "someone@example.com", hasher.HashPassword("correct-password"), DateTimeOffset.UtcNow);
        repository.Seed(user);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => useCase.ExecuteAsync(new LoginUserRequest("someone@example.com", "wrong-password"), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownEmail_ThrowsAuthenticationFailedException()
    {
        var useCase = CreateUseCase(out _);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => useCase.ExecuteAsync(new LoginUserRequest("nobody@example.com", "whatever"), CancellationToken.None));
    }
}
