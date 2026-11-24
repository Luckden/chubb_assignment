using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDb' is not configured.");

// --- Persistence -----------------------------------------------------------
builder.Services.AddDbContext<ClaimsDbContext>(options => options.UseNpgsql(connectionString));

// --- Messaging (Kafka) -----------------------------------------------------
builder.Services.AddKafkaMessaging(builder.Configuration);

// --- Web -------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

const string FrontendCorsPolicy = "frontend";
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// --- Health ----------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

// TODO: apply EF Core migrations on startup (dev only) once the first migration exists.

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
