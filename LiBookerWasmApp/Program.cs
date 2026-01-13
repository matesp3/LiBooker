using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using LiBooker.Blazor.Client.Services;


namespace LiBookerWasmApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // Resolve API base from configuration (wwwroot/appsettings.json) or fallback to host base address.
            var apiBase = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(apiBase))
            {
                apiBase = builder.HostEnvironment.BaseAddress;
            }

            // Typed client: PersonClient receives HttpClient via constructor
            builder.Services.AddHttpClient<PersonClient>(client =>
            {
                client.BaseAddress = new Uri(apiBase);
            });

            // If you plan to use authorization in components
            builder.Services.AddAuthorizationCore();

            // Build host so we can use IJSRuntime
            var host = builder.Build();

            // Log to browser console (visible in DevTools -> Console)
            var js = host.Services.GetRequiredService<IJSRuntime>();
            await js.InvokeVoidAsync("console.log", $"Resolved ApiBaseUrl: '{apiBase}'");

            await host.RunAsync();
        }
    }
}
