using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Application.Users;
using Xunit;

namespace NotifyMe.Application.Tests.Users;

public class RegisterUserUseCaseTests
{
    private static RegisterUserUseCase CreateUseCase(out InMemoryUserRepository repository)
    {
        repository = new InMemoryUserRepository();
        return new RegisterUserUseCase(
            repository, new FakePasswordHasher(), new FakeJwtTokenGenerator(), new RegisterUserRequestValidator());
    }

    [Fact]
    public async Task ExecuteAsync_WithNewEmail_CreatesUserAndReturnsToken()
    {
        var useCase = CreateUseCase(out var repository);
        var request = new RegisterUserRequest("someone@example.com", "a-strong-password");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal("someone@example.com", result.User.Email);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(await repository.GetByEmailAsync("someone@example.com", CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyRegisteredEmail_ThrowsConflictException()
    {
        var useCase = CreateUseCase(out var repository);
        var request = new RegisterUserRequest("someone@example.com", "a-strong-password");
        await useCase.ExecuteAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => useCase.ExecuteAsync(new RegisterUserRequest("Someone@Example.com", "another-password"), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithWeakPassword_ThrowsValidationException()
    {
        var useCase = CreateUseCase(out _);
        var request = new RegisterUserRequest("someone@example.com", "short");

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => useCase.ExecuteAsync(request, CancellationToken.None));
    }
}
