using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotifyMe.Api.Authentication;
using NotifyMe.Api.Endpoints;
using NotifyMe.Api.ErrorHandling;
using NotifyMe.Api.Workers;
using NotifyMe.Application;
using NotifyMe.Application.Abstractions;
using NotifyMe.Infrastructure.EventSources;
using NotifyMe.Infrastructure.EventSources.Simulated;
using NotifyMe.Infrastructure.NotificationChannels;
using NotifyMe.Infrastructure.Persistence;
using NotifyMe.Infrastructure.Security;
using Serilog;
using System.Text;

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

// The connection string is resolved lazily from `builder.Configuration` inside
// `AddNotifyMePersistence` (not eagerly here) so that `WebApplicationFactory`-based integration
// tests' `ConfigureAppConfiguration` overrides - only merged into the final configuration during
// `builder.Build()` - are picked up correctly. See the doc comment on
// `PersistenceServiceCollectionExtensions.AddNotifyMePersistence` for the full explanation.
builder.Services.AddNotifyMePersistence(builder.Configuration);
builder.Services.AddNotifyMeSecurity();
builder.Services.AddSimulatedEventSource(builder.Configuration);
builder.Services.AddNotifyMeNotificationChannels(builder.Configuration);
builder.Services.AddNotifyMeApplication(builder.Configuration);
builder.Services.AddHostedService<EventIngestionWorker>();

builder.Services.AddScoped<ApiKeyEndpointFilter>();
builder.Services.AddExceptionHandler<NotifyMeExceptionHandler>();
builder.Services.AddProblemDetails();

// End-user self-service auth (Phase 16, ADR-0010): a JWT bearer scheme entirely separate from
// the Admin API key above. `Jwt:SigningKey` comes from `dotnet user-secrets` locally / an
// environment variable in CI-deployment, never committed, exactly like `Admin:ApiKey`.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// `TokenValidationParameters` is built from `IOptions<JwtOptions>` resolved via DI *after*
// `builder.Build()` (through `AddOptions<JwtBearerOptions>().Configure<...>(...)`) rather than by
// reading `builder.Configuration` directly here. `WebApplicationFactory`-based integration tests
// only merge their `ConfigureAppConfiguration` overrides into the final configuration during
// `builder.Build()`, so capturing `Jwt:SigningKey` before that point would silently use an empty
// key (and mint validation parameters that can never match tokens signed with the real key).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                string.IsNullOrEmpty(jwtOptions.SigningKey) ? Guid.NewGuid().ToString("N") : jwtOptions.SigningKey)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotifyMeDbContext>("postgres");

// CORS for the local frontend dev server (see ai/decisions/adr/0009-frontend-stack.md). The
// origin is configuration-driven (not hardcoded) so it can be tightened/changed per environment;
// defaults to Vite's default dev port for a zero-config local frontend setup.
const string FrontendCorsPolicy = "FrontendCorsPolicy";
var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(frontendOrigin)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapAuthEndpoints();

var adminApi = app.MapGroup("/api/admin").AddEndpointFilter<ApiKeyEndpointFilter>();
adminApi.MapAlertRulesEndpoints();
adminApi.MapChannelsEndpoints();
adminApi.MapSubscriptionsEndpoints();
adminApi.MapNotificationsEndpoints();
adminApi.MapEventsEndpoints();

var myApi = app.MapGroup("/api/me").RequireAuthorization();
myApi.MapMyAlertRulesEndpoints();
myApi.MapMyChannelsEndpoints();
myApi.MapMySubscriptionsEndpoints();

app.Run();

public partial class Program
{
    // Exposed so NotifyMe.IntegrationTests can reference the entry point via WebApplicationFactory<Program>.
}


