using LiBookerWebApi.Model;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace LiBookerWebApi.Infrastructure
{
    public static class OracleExtensions
    {
        /// <summary>
        /// Adds and configures the Oracle database context for the application using settings from the specified
        /// configuration.
        /// </summary>
        /// <remarks>This method reads Oracle database connection settings from the "Oracle" section of
        /// the provided <paramref name="configuration"/>. It sets up the necessary environment variables for Oracle
        /// wallet-based authentication and registers <see cref="AppDbContext"/> with the dependency injection
        /// container, configured to use Oracle Database 19 compatibility. <para> The "Oracle" configuration section
        /// must contain the following keys: "WalletPath", "UserId", "Password", and "TnsAlias". </para></remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Oracle database context will be added.</param>
        /// <param name="configuration">The application configuration containing the Oracle connection settings. Must include the "Oracle" section
        /// with required keys.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
        public static IServiceCollection AddOracleDb(this IServiceCollection services, IConfiguration configuration)
        {
            var oracleCfg = configuration.GetSection("Oracle");

            // Prefer configuration (appsettings / user-secrets), fallback to environment variables.
            var walletPath = oracleCfg["WalletPath"] ?? Environment.GetEnvironmentVariable("ORACLE_WALLET_PATH");
            var userId = oracleCfg["UserId"] ?? Environment.GetEnvironmentVariable("ORACLE_USERID");
            var password = oracleCfg["Password"] ?? Environment.GetEnvironmentVariable("ORACLE_PASSWORD");
            var tnsAlias = oracleCfg["TnsAlias"] ?? Environment.GetEnvironmentVariable("ORACLE_TNSALIAS");

            if (string.IsNullOrWhiteSpace(walletPath))
                throw new InvalidOperationException("Oracle WalletPath is not configured (Oracle:WalletPath or ORACLE_WALLET_PATH).");
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("Oracle UserId is not configured (Oracle:UserId or ORACLE_USERID).");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Oracle Password is not configured (Oracle:Password or ORACLE_PASSWORD).");
            if (string.IsNullOrWhiteSpace(tnsAlias))
                throw new InvalidOperationException("Oracle TnsAlias is not configured (Oracle:TnsAlias or ORACLE_TNSALIAS).");

            walletPath = Path.GetFullPath(walletPath);

            // Wallet must be set before DbContext creation
            Environment.SetEnvironmentVariable("TNS_ADMIN", walletPath);
            OracleConfiguration.TnsAdmin = walletPath;

            var connectionString =
                $"User Id={userId};" +
                $"Password={password};" +
                $"Data Source={tnsAlias};" +
                "Pooling=true;" +               // Enabling pooling (made 5% difference in tests for second request)
                "Min Pool Size=2;" +            // Keep 2 connections always open (warm pool)
                "Max Pool Size=20;" +           // Allow up to 20 concurrent connections
                "Incr Pool Size=2;" +           // Grow pool by 2 when needed
                "Decr Pool Size=1;" +           // Shrink pool slowly
                "Connection Lifetime=300;" +    // Recycle connections every 5 min (avoid stale connections)
                "Connection Timeout=60;" +      // Timeout for new connections
                "Validate Connection=true;";    // Validate before reuse (Oracle-specific)

            // Register a DbContext factory (singleton-friendly)
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseOracle(
                    connectionString,
                    oracle =>
                    {
                        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                    });
            });

            // Provide a scoped AppDbContext created from the factory for normal request-scoped DI consumers.
            services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                return factory.CreateDbContext();
            });
            // scoped means one instance per request
            // transient would mean a new instance every time it's requested
            // singleton would mean one instance for the entire application lifetime

            return services;
        }
    }

    /*
     * When to inject AppDbContext (scoped) vs. use the factory
        • Endpoints (HTTP request handlers) and other request-scoped services: continue injecting AppDbContext directly (scoped). This keeps lambdas simple and aligns with DI scope.
        • Background services, singletons, worker threads, or any code that outlives the request scope: use IDbContextFactory<TContext>.
       Best practices
        • Dispose contexts promptly (use using / await using).
        • Pass CancellationToken to EF methods and CreateDbContextAsync when available.
        • Use AsNoTracking() for read-only queries.
        • Prefer transactions for multi-step writes and create the context inside the transaction scope.
        • Do not attempt to share a DbContext instance across threads.
     */
}
