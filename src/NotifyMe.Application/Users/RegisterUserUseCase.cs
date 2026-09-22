using FluentValidation;
using NotifyMe.Application.Abstractions;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Users;

namespace NotifyMe.Application.Users;

/// <summary>
/// Registers a new end user and immediately logs them in (returns a token), so the frontend's
/// register form doesn't need a second login round trip. See ADR-0010 for the auth model.
/// </summary>
public sealed class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterUserRequest> _validator;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterUserRequest> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _validator = validator;
    }

    public async Task<AuthResultDto> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = User.Normalize(request.Email);
        var existing = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException($"A user with email '{normalizedEmail}' is already registered.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(Guid.NewGuid(), normalizedEmail, passwordHash, DateTimeOffset.UtcNow);

        await _userRepository.AddAsync(user, cancellationToken);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto(token, expiresAt, new UserDto(user.Id, user.Email, user.CreatedAt));
    }
}
