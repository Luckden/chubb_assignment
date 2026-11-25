using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Application.Exposure;
using Claims.Api.Application.Notifications;
using Claims.Api.Application.Workload;
using Claims.Api.Controllers;
using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using Claims.Api.Infrastructure.Persistence.Seeders;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDb' is not configured.");

// --- Persistence -----------------------------------------------------------
builder.Services.AddDbContext<ClaimsDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
// Seeders run on every startup when Database:MigrateOnStartup is true, so they must be idempotent.
builder.Services.AddScoped<IDataSeeder, DemoUserSeeder>();
builder.Services.AddScoped<IDataSeeder, SampleDataSeeder>(); // does nothing unless Database:SeedSampleData is true

// --- Application -----------------------------------------------------------
// Validators (AbstractValidator<T>) are discovered from this assembly. Call them from application services.
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ClaimService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<WorkloadService>();
builder.Services.AddScoped<ExposureService>();
builder.Services.AddScoped<IEventHandler, ClaimantNotificationHandler>();

// --- Messaging (Kafka) -----------------------------------------------------
builder.Services.AddKafkaMessaging(builder.Configuration);

// --- Web -------------------------------------------------------------------
// Enums travel as strings (readable JSON and a readable OpenAPI schema).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// The OpenAPI generator reads the HTTP JSON options, not the MVC ones above: without this the schema says integer.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

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
