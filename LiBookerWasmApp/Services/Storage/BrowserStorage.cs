using Microsoft.JSInterop;

namespace LiBookerWasmApp.Services.Storage
{
    public interface IBrowserStorage
    {
        Task<string?> GetItemAsync(string key);
        Task SetItemAsync(string key, string value);
        Task RemoveItemAsync(string key);
    }

    public class BrowserStorage : IBrowserStorage
    {
        private readonly IJSRuntime jsRuntime;

        public BrowserStorage(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            }
            catch
            {
                // in case when storage is unavailable (e.g. private mode in some browsers)
                return null;
            }
        }

        public async Task SetItemAsync(string key, string value)
        {
            await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public async Task RemoveItemAsync(string key)
        {
            await this.jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}