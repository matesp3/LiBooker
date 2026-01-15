using LiBookerWebApi.Endpoints;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

string corsPolicy = string.Empty;
if (builder.Environment.IsDevelopment())
{
    corsPolicy = ConfigureDevCors(builder);
}

// existing method that configures Oracle/DbContext
builder.Services.AddOracleDb(builder.Configuration, out var connectionString);

// register scoped services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IPublicationService, PublicationService>();

var app = builder.Build();

// Warm up Oracle connection pool during startup (non-blocking)
LaunchConnectionPoolWarmup(app.Services, connectionString, app.Environment.IsDevelopment());

app.UseHttpsRedirection();
app.UseAuthorization();

bool logDuration = IsDurationLoggingEnabled(app);
app.MapPersonEndpoints();
app.MapPublicationEndpoints(logDuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    if (corsPolicy != string.Empty)
        app.UseCors(corsPolicy);
}

app.Run();


static string ConfigureDevCors(WebApplicationBuilder builder)
{
    var cfg = builder.Configuration.GetSection("CORS");
    string wasmOrigin = cfg["WASM origin"] ?? "https://localhost:7192";

    string corsPolicyName = "AllowLocalBlazor";
    // Configure CORS to allow the Blazor WASM origin to make API calls during development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.WithOrigins(wasmOrigin) // my WASM origin
              .AllowAnyHeader()
              .AllowAnyMethod();
            // .AllowCredentials(); // only if when I need cookies/auth; then don't use AllowAnyOrigin()
        });
    });
    return corsPolicyName;
}

/// <summary>
/// Whether duration logging (on console) is enabled via configuration.
/// </summary>
static bool IsDurationLoggingEnabled(WebApplication app)
{
    var cfg = app.Configuration.GetSection("Performance");
    string val = (cfg["LogDuration"]?.ToLower() ?? "");
    return val switch
    {
        "1" or "true" or "yes" or "on" => true,
        _ => false,
    };
}

/// <summary>
/// Non-blocking launch of connection pool warm-up
/// <summary/>
static void LaunchConnectionPoolWarmup(IServiceProvider services, string connectionString, bool logDetails)
{
    _ = Task.Run(async () =>
    {
        try
        {
            await services.WarmUpOracleConnectionPoolAsync(connectionString, logDetails);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Connection pool warm-up failed during startup");
        }
    });
}
