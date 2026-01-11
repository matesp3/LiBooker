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

            var walletPath = Path.GetFullPath(oracleCfg["WalletPath"]!);

            // Wallet must be set before DbContext
            Environment.SetEnvironmentVariable("TNS_ADMIN", walletPath);
            OracleConfiguration.TnsAdmin = walletPath;

            var connectionString =
                $"User Id={oracleCfg["UserId"]};" +
                $"Password={oracleCfg["Password"]};" +
                $"Data Source={oracleCfg["TnsAlias"]};" +
                "Pooling=false;" +
                "Connection Timeout=60;";

            // for diagnostics
            //Console.WriteLine("TNS_ADMIN = " + walletPath);
            //Console.WriteLine("TNS_ADMIN exists: " + Directory.Exists(walletPath));
            //Console.WriteLine("cwallet.sso exists: " + File.Exists(Path.Combine(walletPath, "cwallet.sso")));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseOracle(
                    connectionString,
                    oracle =>
                    {
                        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                    });
            });

            // Also register a factory for creating contexts on-demand (background/singleton)
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseOracle(
                    connectionString,
                    oracle =>
                    {
                        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                    });
            });

            return services;
        }
    }

    /*
     * When to inject AppDbContext (scoped) vs. use the factory
        •	Endpoints (HTTP request handlers) and other request-scoped services: continue injecting AppDbContext directly (scoped). This keeps lambdas simple and aligns with DI scope.
        •	Background services, singletons, worker threads, or any code that outlives the request scope: use IDbContextFactory<TContext>.
       Best practices
        •	Dispose contexts promptly (use using / await using).
        •	Pass CancellationToken to EF methods and CreateDbContextAsync when available.
        •	Use AsNoTracking() for read-only queries.
        •	Prefer transactions for multi-step writes and create the context inside the transaction scope.
        •	Do not attempt to share a DbContext instance across threads.
     */

}
