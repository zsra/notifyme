var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints are added starting in Phase 08 (API layer). This is intentionally a bare host for
// now; see ai/plan/phase-08-api-layer.md.

app.Run();

public partial class Program
{
    // Exposed so NotifyMe.IntegrationTests can reference the entry point via WebApplicationFactory<Program>.
}
