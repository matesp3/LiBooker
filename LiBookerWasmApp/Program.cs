using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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

            // Scoped state/services
            //builder.Services.AddScoped<PersonState>(); // TODO CO S TYM

            // If you plan to use authorization in components
            builder.Services.AddAuthorizationCore();

            await builder.Build().RunAsync();
        }
    }
}
