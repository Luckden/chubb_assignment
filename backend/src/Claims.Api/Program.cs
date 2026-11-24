using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDb' is not configured.");

// --- Persistence -----------------------------------------------------------
builder.Services.AddDbContext<ClaimsDbContext>(options => options.UseNpgsql(connectionString));
// Seeders: register implementations of IDataSeeder here, e.g. builder.Services.AddScoped<IDataSeeder, DemoDataSeeder>();

// --- Application -----------------------------------------------------------
// Validators (AbstractValidator<T>) are discovered from this assembly. Call them from application services.
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

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

// Applies migrations and runs seeders when Database:MigrateOnStartup is true (Development / tests).
await app.InitializeDatabaseAsync();

await app.RunAsync();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
