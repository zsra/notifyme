using Microsoft.Extensions.DependencyInjection;
using NotifyMe.Application.Abstractions;

namespace NotifyMe.Infrastructure.Security;

/// <summary>
/// Registers the Infrastructure-layer security primitives used by the Phase 16 self-service
/// auth flow. Separate from `AddNotifyMePersistence` since password hashing has nothing to do
/// with the database, it's just grouped under `Infrastructure` because it's a concrete
/// implementation of an `Application`-declared abstraction (see ADR-0010).
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyMeSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
