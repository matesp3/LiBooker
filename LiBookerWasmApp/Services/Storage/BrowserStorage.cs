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
        private readonly IJSRuntime _jsRuntime;

        public BrowserStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            }
            catch
            {
                // in case when _storage is unavailable (e.g. private mode in some browsers)
                return null;
            }
        }

        public async Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}