namespace NotifyMe.Application.Users;

public sealed record UserDto(Guid Id, string Email, DateTimeOffset CreatedAt);
