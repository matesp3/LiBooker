using LiBooker.Shared.DTOs.Admin;
using LiBookerWasmApp.Services.Clients;
using LiBookerWasmApp.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LiBookerWasmApp.Pages.Admin.Components
{
    public partial class UserSearchInput : IDisposable
    {
        [Parameter]
        public EventCallback<string> OnSearchConfirmed { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = "search...";

        [Inject]
        public required UserClient UserClient { get; set; }

        private string searchTerm = "";
        private bool showSuggestions = false;
        private List<UserManagement> suggestions = [];
        private System.Timers.Timer? debounceTimer;
        private CancellationTokenSource? searchCts = null;

        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                if (this.searchTerm != value)
                {
                    this.searchTerm = value;
                    OnSearchInput(); // Trigger debounce on change
                }
            }
        }

        private void OnSearchInput()
        {
            // Stop and dispose previous timer to prevent multiple timers running
            this.debounceTimer?.Stop();
            this.debounceTimer?.Dispose();

            if (string.IsNullOrWhiteSpace(this.searchTerm))
            {
                this.suggestions.Clear();
                this.showSuggestions = false;
                return;
            }

            // Create new timer
            this.debounceTimer = new System.Timers.Timer(300);
            this.debounceTimer.AutoReset = false; // Run only once!
            this.debounceTimer.Elapsed += async (_, _) => 
            {
                await InvokeAsync(async () => await LoadSuggestions()); 
            };
            this.debounceTimer.Start();
        }

        private async Task LoadSuggestions()
        {
            // cancel previous API request if it's still running
            this.searchCts?.Cancel();
            //this.searchCts?.Dispose();
            this.searchCts = new CancellationTokenSource();
            var token = this.searchCts.Token;

            try
            {
                var res = await this.UserClient.SearchUsersByEmailAsync(this.searchTerm, token);
                
                if (!token.IsCancellationRequested && res.IsSuccess)
                {
                    this.suggestions = res.Data?.Take(10).ToList() ?? [];
                    this.showSuggestions = this.suggestions.Count > 0; // Show only if we have data
                    StateHasChanged();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected behavior when typing fast
            }
            // update only if we actually got results or cleared them.
        }

        private async Task SelectSuggestion(UserManagement user)
        {
            this.searchTerm = user.Email;
            this.showSuggestions = false;
            if (this.OnSearchConfirmed.HasDelegate)
            {
                await this.OnSearchConfirmed.InvokeAsync(this.searchTerm);
            }
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                this.showSuggestions = false;
                if (this.OnSearchConfirmed.HasDelegate)
                {
                    await this.OnSearchConfirmed.InvokeAsync(this.searchTerm);
                }
            }
        }

        private void ClearSearch()
        {
            this.searchTerm = "";
            this.showSuggestions = false;
            if (this.OnSearchConfirmed.HasDelegate) // notify parent that search is cleared
            {
                this.OnSearchConfirmed.InvokeAsync("");
            }
        }

        // Helper for highlighting text
        private MarkupString Highlight(string text)
        {
            return Highlighter.Highlight(this.searchTerm, text);
        }

        private async Task OnSearchBlur()
        {
            // Give time for click event to register before hiding
            await Task.Delay(200);
            this.showSuggestions = false;
            // Force re-render to hide the dropdown
            StateHasChanged(); 
        }

        public void Dispose()
        {
            this.debounceTimer?.Stop();
            this.debounceTimer?.Dispose();
            this.searchCts?.Cancel();
            //this.searchCts?.Dispose();
            try
            {
                this.searchCts?.Cancel();
                //this.searchCts?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
