using NotifyMe.Api.Authentication;
using NotifyMe.Api.Endpoints;
using NotifyMe.Api.ErrorHandling;
using NotifyMe.Application;
using NotifyMe.Infrastructure.EventSources;
using NotifyMe.Infrastructure.NotificationChannels;
using NotifyMe.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration["NOTIFYME_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only";

builder.Services.AddNotifyMePersistence(connectionString);
builder.Services.AddSimulatedEventSource(builder.Configuration);
builder.Services.AddNotifyMeNotificationChannels(builder.Configuration);
builder.Services.AddNotifyMeApplication();

builder.Services.AddScoped<ApiKeyEndpointFilter>();
builder.Services.AddExceptionHandler<NotifyMeExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

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

