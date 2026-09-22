using NotifyMe.Application.Users;

namespace NotifyMe.Api.Endpoints;

/// <summary>
/// Public (unauthenticated) endpoints for end-user registration and login, per ADR-0010. These
/// live under `/api/auth`, separate from both the Admin API key group and the JWT-protected
/// `/api/me` group.
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterUserRequest request, RegisterUserUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request, cancellationToken)))
            .Produces<AuthResultDto>()
            .AllowAnonymous();

        group.MapPost("/login", async (
            LoginUserRequest request, LoginUserUseCase useCase, CancellationToken cancellationToken) =>
            Results.Ok(await useCase.ExecuteAsync(request, cancellationToken)))
            .Produces<AuthResultDto>()
            .AllowAnonymous();
    }
}
