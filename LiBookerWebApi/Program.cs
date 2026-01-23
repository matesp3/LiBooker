using LiBooker.Shared.DTOs;
using LiBooker.Shared.Roles;
using LiBookerWebApi.Endpoints;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Model;
using LiBookerWebApi.Models;
using LiBookerWebApi.Services;
using LiBookerWebApi.Utils;
using Microsoft.AspNetCore.Identity;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.RequireLoggedUser, policy => policy.RequireRole(UserRoles.User.GetRoleName()))
            .AddPolicy(AuthPolicies.RequireBlogger, policy => policy.RequireRole(UserRoles.Blogger.GetRoleName()))
            .AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole(UserRoles.Admin.GetRoleName()));

        string corsPolicy = string.Empty;
        if (builder.Environment.IsDevelopment())
        {
            corsPolicy = ConfigureDevCors(builder);
        }

        // existing method that configures Oracle/DbContext
        builder.Services.AddOracleDb(builder.Configuration, out var connectionString);

        // Configure ASP.NET Core Identity
        builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<LiBookerDbContext>();

        // register scoped services
        builder.Services.AddScoped<IPersonService, PersonService>();
        builder.Services.AddScoped<IPublicationService, PublicationService>();
        builder.Services.AddScoped<IMatchSearchService, MatchSearchService>();
        builder.Services.AddScoped<IBookService, BookService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ILoanService, LoanService>();
        

        var app = builder.Build();

        // Warm up Oracle connection pool during startup (non-blocking)
        LaunchConnectionPoolWarmup(app.Services, connectionString, app.Environment.IsDevelopment());

        app.UseHttpsRedirection(); // use encryption only way

        // CORS must be called before authentication and authorization
        if (app.Environment.IsDevelopment() && corsPolicy != string.Empty)
        {
            app.UseCors(corsPolicy);
        }

        app.UseAuthentication();
        app.UseAuthorization();

        bool logDuration = IsDurationLoggingEnabled(app);
        app.MapPersonEndpoints();
        app.MapPublicationEndpoints(logDuration);
        app.MapMatchSearchEndpoint(logDuration);
        app.MapBookEndpoints();
        app.MapRegistrationEndpoints();
        app.MapUserEndpoints();
        app.MapLoanEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                await EnsureUserRolesExist(scope);
                await EnsureAdminUserExists(scope);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding roles or admin user.");
                throw; // rethrow to prevent app from starting in invalid state
            }
            try // Optional: process additional command-line arguments
            {
                CreateUsersIfNeeded(args, scope, logger, app);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during CLI arguments processing.");
                throw; // rethrow to prevent app from starting in invalid state
            }
        }

        app.Run();

        // ###########___ START OF AN APP ___##############
    }

    /// Ensures that all required user roles exist in the database.
    async static Task EnsureUserRolesExist(IServiceScope scope)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = UserRolesExtensions.GetAllRoleNames().ToArray();
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                await roleManager.CreateAsync(role);
            }
        }
    }

    async static Task EnsureAdminUserExists(IServiceScope scope)
    {
        // Retrieve IConfiguration from the service provider
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string? adminEmail = config["LIBOOKER_USER_ADMIN_EMAIL"];
        if (string.IsNullOrEmpty(adminEmail))
        {
            throw new InvalidOperationException("Configuration 'LIBOOKER_USER_ADMIN_EMAIL' is missing.");
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var adminPerson = new PersonRegistration
            {
                FirstName = "Admin",
                LastName = "Admin",
                BirthDate = DateTime.Now,
                Email = adminEmail,
                Gender = 'M',
                Password = config["LIBOOKER_USER_ADMIN_PASSWORD"] ?? throw new InvalidOperationException("Environment variable 'LIBOOKER_USER_ADMIN_PASSWORD' is not set."),
            };
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var res = await authService.RegisterUserAsync(adminPerson, default);
            if (!res.IsSuccessful)
                throw new InvalidProgramException($"Admin user creation failed at {DateTime.Now}. Reason: {res.FailureReason}");
        }
        else // ensuring that existing admin user has admin privileges
        {
            var adminRoleName = UserRoles.Admin.GetRoleName();
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }

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
        string val = cfg["LogDuration"]?.ToLower() ?? "";
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

    static void CreateUsersIfNeeded(string[] args, IServiceScope scope, ILogger<Program> logger, WebApplication app)
    {
        // 1. Základná validácia argumentov
        if (args.Length < 3 || args[0] != "--persons_upload" || args[1] != "-f")
            return;

        string filePath = args[2];
        bool shouldExit = args.Length == 4 && args[3] == "--exit_after_upload";
        var authSvc = scope.ServiceProvider.GetRequiredService<IAuthService>();

        logger.LogInformation("Starting person upload from: {FilePath}", filePath);

        async Task ExecuteUpload()
        {
            try
            {
                await PersonUploader.UploadPersonsFromExportFile(filePath, authSvc, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Person upload failed.");
                if (shouldExit) Environment.Exit(1); // Ak bežíme ako CLI tool, vrátime error code
            }
        }

        if (shouldExit) // way of running as a CLI tool
        {
            // blocking call - application won't start as web server, but will exit after upload
            ExecuteUpload().GetAwaiter().GetResult();
            logger.LogInformation("Upload finished. Exiting application as requested.");
            Environment.Exit(0);
        }
        else // running in background alongside the web server
        {   // warning: exceptions here won't crash the app, so we log them
            _ = Task.Run(async () =>
            {
                using var internalScope = app.Services.CreateScope();
                var internalAuthSvc = internalScope.ServiceProvider.GetRequiredService<IAuthService>();
                var internalLogger = internalScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    await PersonUploader.UploadPersonsFromExportFile(filePath, internalAuthSvc, internalLogger);
                }
                catch (Exception ex)
                {
                    internalLogger.LogError(ex, "Background upload failed.");
                }
            });
        }
    }
}