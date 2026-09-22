using NotifyMe.Api.Authentication;
using NotifyMe.Api.Endpoints;
using NotifyMe.Api.ErrorHandling;
using NotifyMe.Api.Workers;
using NotifyMe.Application;
using NotifyMe.Infrastructure.EventSources;
using NotifyMe.Infrastructure.EventSources.Simulated;
using NotifyMe.Infrastructure.NotificationChannels;
using NotifyMe.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// A static/bootstrap `Log.Logger` (the usual Serilog+minimal-API pattern) is deliberately not
// used here: it gets frozen the first time a host built from it is disposed, which breaks
// `WebApplicationFactory<Program>`-based integration tests that build (and rebuild) this same
// entry point's host multiple times within one test process. Configuring Serilog purely through
// `UseSerilog` below gives each host its own logger instance instead.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration["NOTIFYME_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only";

builder.Services.AddNotifyMePersistence(connectionString);
builder.Services.AddSimulatedEventSource(builder.Configuration);
builder.Services.AddNotifyMeNotificationChannels(builder.Configuration);
builder.Services.AddNotifyMeApplication(builder.Configuration);
builder.Services.AddHostedService<EventIngestionWorker>();

builder.Services.AddScoped<ApiKeyEndpointFilter>();
builder.Services.AddExceptionHandler<NotifyMeExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotifyMeDbContext>("postgres");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");

var adminApi = app.MapGroup("/api/admin").AddEndpointFilter<ApiKeyEndpointFilter>();
adminApi.MapAlertRulesEndpoints();
adminApi.MapChannelsEndpoints();
adminApi.MapSubscriptionsEndpoints();
adminApi.MapNotificationsEndpoints();
adminApi.MapEventsEndpoints();

app.Run();

public partial class Program
{
    // Exposed so NotifyMe.IntegrationTests can reference the entry point via WebApplicationFactory<Program>.
}


