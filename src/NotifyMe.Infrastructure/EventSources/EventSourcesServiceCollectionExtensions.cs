using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Infrastructure.EventSources.Simulated;

namespace NotifyMe.Infrastructure.EventSources;

/// <summary>
/// Registers the event-ingestion seam. Today this always wires up <see cref="SimulatedEventSource"/>;
/// swapping in a real source (see EventSources/External/README.md) means adding a new extension
/// method here (or an alternative overload) rather than touching Domain or Application.
/// </summary>
public static class EventSourcesServiceCollectionExtensions
{
    public static IServiceCollection AddSimulatedEventSource(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SimulatedEventSourceOptions>(configuration.GetSection(SimulatedEventSourceOptions.SectionName));
        services.AddSingleton<IEventSource, SimulatedEventSource>();

        return services;
    }
}
