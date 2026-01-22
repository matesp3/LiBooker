using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using LiBookerWasmApp.Services.Clients;
using LiBookerWasmApp.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using LiBookerWasmApp.Services.Auth;

namespace LiBookerWasmApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            string apiBase = ConfigureApiServices(builder);
    
            // 0. registering own handler
            builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

            // 1. registration of our own _storage service
            builder.Services.AddScoped<IBrowserStorage, BrowserStorage>();

            // 2. Auth Core
            builder.Services.AddAuthorizationCore();

            // 3. Custom Provider (depends on IBrowserStorage)
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            // 4. Registration of specific class for DI
            builder.Services.AddScoped(sp => 
                (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
            
            // Build host so we can use IJSRuntime
            var host = builder.Build();

            // Global exception handlers: log to browser console immediately
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    Console.WriteLine($"[GLOBAL UNHANDLED] {e.ExceptionObject}");
                }
                catch { }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try
                {
                    Console.WriteLine($"[UNOBSERVED TASK] {e.Exception}");
                    e.SetObserved();
                }
                catch { }
            };

            // Log to browser console (visible in DevTools -> Console)
            var js = host.Services.GetRequiredService<IJSRuntime>();
            await js.InvokeVoidAsync("console.log", $"Resolved ApiBaseUrl: '{apiBase}'");

            await host.RunAsync();
        }

        private static string ConfigureApiServices(WebAssemblyHostBuilder builder)
        {
            // Resolve API base from configuration (wwwroot/appsettings.json) or fallback to host base address.
            var apiBase = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(apiBase))
            {
                apiBase = builder.HostEnvironment.BaseAddress;
            }

            // Named client: for CustomAuthStateProvider
            builder.Services.AddHttpClient("LiBookerApi", client =>
            {
                client.BaseAddress = new Uri(apiBase);
            });

            // Typed clients: XYZ_Client receives HttpClient via constructor
            RegisterClientWithOwnMsgHandler<PersonClient>(builder, apiBase);
            RegisterClientWithOwnMsgHandler<PublicationClient>(builder, apiBase);
            RegisterClientWithOwnMsgHandler<BookClient>(builder, apiBase);
            RegisterClientWithOwnMsgHandler<UserClient>(builder, apiBase);

            return apiBase;

            static void RegisterClientWithOwnMsgHandler<T>(WebAssemblyHostBuilder builder, string apiBase)
                where T : class, ICustomClient
            {
                builder.Services.AddHttpClient<T>(client =>
                {
                    client.BaseAddress = new Uri(apiBase);
                })
                .AddHttpMessageHandler<CustomAuthorizationMessageHandler>(); // important for authorization purpose
            }
        }
    }
}
