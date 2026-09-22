using FluentValidation;
using NotifyMe.Application.Abstractions;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;
using NotifyMe.Domain.Users;

namespace NotifyMe.Application.Users;

public sealed class LoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginUserRequest> _validator;

    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginUserRequest> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _validator = validator;
    }

    public async Task<AuthResultDto> ExecuteAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = User.Normalize(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto(token, expiresAt, new UserDto(user.Id, user.Email, user.CreatedAt));
    }
}
