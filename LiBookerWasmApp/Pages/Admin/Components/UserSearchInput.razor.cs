using LiBooker.Shared.DTOs.Admin;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LiBookerWasmApp.Pages.Admin.Components
{
    public partial class UserSearchInput : IDisposable
    {
        /// <summary>
        /// Fired when a search is confirmed via Enter or suggestion selection (using the email string).
        /// Maintains compatibility with search-based pages (e.g. RoleManager).
        /// </summary>
        [Parameter] public EventCallback<string> OnSearchConfirmed { get; set; }

        /// <summary>
        /// Fired when a specific user suggestion is selected.
        /// Provides the full UserManagement object for pages that need rich data (id, roles, etc.).
        /// </summary>
        [Parameter] public EventCallback<UserManagement> OnUserSelected { get; set; }

        [Parameter] public string Placeholder { get; set; } = "search...";

        [Inject] public required UserClient UserClient { get; set; }
        [Inject] public required IJSRuntime JS { get; set; }

        private string searchTerm = "";
        private bool showSuggestions = false;
        private List<UserManagement> suggestions = new();
        private System.Timers.Timer? debounceTimer;
        private CancellationTokenSource? searchCts;
        private bool isLoadingSuggestions = false;
        private bool suppressNextBlurConfirm = false;

        // ids for DOM elements used by JS positioning
        private readonly string InputId = $"usersearch_input_{Guid.NewGuid():N}";
        private readonly string DropdownId = $"usersearch_dropdown_{Guid.NewGuid():N}";

        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                if (this.searchTerm != value)
                {
                    this.searchTerm = value;
                    OnSearchInput(); // debounce-driven suggestion fetch
                }
            }
        }

        private void OnSearchInput()
        {
            // cancel previous timer
            try
            {
                this.debounceTimer?.Stop();
                this.debounceTimer?.Dispose();
            }
            catch { /* ignore */ }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // cancel any pending request too (thread-safe)
                CancelCurrentSearchCts();

                this.suggestions.Clear();
                this.showSuggestions = false;
                this.isLoadingSuggestions = false;
                StateHasChanged();
                return;
            }

            this.debounceTimer = new System.Timers.Timer(300);
            this.debounceTimer.AutoReset = false; // run only once
            this.debounceTimer.Elapsed += async (_, _) =>
            {
                await InvokeAsync(async () => await LoadSuggestionsAsync());
            };
            this.debounceTimer.Start();
        }

        private CancellationTokenSource SwapInNewSearchCts()
        {
            var newCts = new CancellationTokenSource();
            var prev = Interlocked.Exchange(ref this.searchCts, newCts);
            if (prev != null)
            {
                try { prev.Cancel(); }
                catch (ObjectDisposedException) { }
                catch { }

                try { prev.Dispose(); }
                catch { }
            }
            return newCts;
        }

        private void CancelCurrentSearchCts()
        {
            var prev = Interlocked.Exchange(ref this.searchCts, null);
            if (prev != null)
            {
                try { prev.Cancel(); }
                catch (ObjectDisposedException) { }
                catch { }

                try { prev.Dispose(); }
                catch { }
            }
        }

        private async Task LoadSuggestionsAsync()
        {
            var cts = SwapInNewSearchCts();
            var token = cts.Token;

            this.isLoadingSuggestions = true;
            StateHasChanged();

            try
            {
                var res = await UserClient.SearchUsersByEmailAsync(this.searchTerm, token);

                if (!token.IsCancellationRequested && res.IsSuccess)
                {
                    this.suggestions = res.Data?.Take(10).ToList() ?? new List<UserManagement>();
                    this.showSuggestions = this.suggestions.Count > 0;
                }
                else
                {
                    this.suggestions.Clear();
                    this.showSuggestions = false;
                }
            }
            catch (OperationCanceledException)
            {
                this.suggestions.Clear();
                this.showSuggestions = false;
            }
            catch (Exception)
            {
                this.suggestions.Clear();
                this.showSuggestions = false;
            }
            finally
            {
                this.isLoadingSuggestions = false;
                StateHasChanged();

                // Position dropdown in viewport if visible
                if (this.showSuggestions)
                {
                    try
                    {
                        // call JS to position dropdown
                        await JS.InvokeVoidAsync("userSearch.setDropdownPosition", DropdownId, InputId);
                    }
                    catch { /* ignore interop errors for robustness */ }
                }
                else
                {
                    // cleanup inline styles if hidden
                    try
                    {
                        await JS.InvokeVoidAsync("userSearch.hideDropdownInlineStyles", DropdownId);
                    }
                    catch { /* ignore */ }
                }
            }
        }

        private async Task SelectSuggestion(UserManagement user)
        {
            // cancel pending suggestion requests to avoid late responses clearing UI
            CancelCurrentSearchCts();

            // prevent blur from firing an extra confirm
            this.suppressNextBlurConfirm = true;

            var trimmed = (user.Email ?? "").Trim();
            this.searchTerm = trimmed;
            this.showSuggestions = false;
            this.suggestions.Clear();

            // 1. Invoke the richer event first (for usages requiring the object)
            if (this.OnUserSelected.HasDelegate)
                await this.OnUserSelected.InvokeAsync(user);

            // 2. Invoke the string compatibility event (for RoleManager or search-based usages)
            if (this.OnSearchConfirmed.HasDelegate)
                await this.OnSearchConfirmed.InvokeAsync(trimmed);

            // small delay to make sure blur handler sees suppress flag if it runs
            await Task.Yield();
            this.suppressNextBlurConfirm = false;
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                // cancel pending suggestion requests to avoid late responses clearing UI
                CancelCurrentSearchCts();

                // prevent blur-based confirmation duplicate
                this.suppressNextBlurConfirm = true;

                this.showSuggestions = false;
                this.suggestions.Clear();

                var trimmed = (searchTerm ?? "").Trim();
                
                if (this.OnSearchConfirmed.HasDelegate)
                    await this.OnSearchConfirmed.InvokeAsync(trimmed);

                // small delay to reset suppression
                await Task.Yield();
                this.suppressNextBlurConfirm = false;
            }
        }

        private void ClearSearch()
        {
            // cancel pending ops
            try
            {
                this.debounceTimer?.Stop();
                this.debounceTimer?.Dispose();
            }
            catch { /* ignore */ }

            CancelCurrentSearchCts();

            this.searchTerm = "";
            this.suggestions.Clear();
            this.showSuggestions = false;

            // notify parent to clear results
            if (this.OnSearchConfirmed.HasDelegate)
                _ = this.OnSearchConfirmed.InvokeAsync("");

            StateHasChanged();
        }

        private MarkupString Highlight(string text)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
                return new MarkupString(text ?? "");
            var pattern = System.Text.RegularExpressions.Regex.Escape(searchTerm);
            var result = System.Text.RegularExpressions.Regex.Replace(text, $"({pattern})", "<strong>$1</strong>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return new MarkupString(result);
        }

        private async Task OnSearchBlur()
        {
            // allow click on suggestion to register (suggestion click sets suppressNextBlurConfirm)
            await Task.Delay(160);

            if (this.suppressNextBlurConfirm)
            {
                // Reset suppression and do not forcibly hide suggestions here.
                // Let click/select handlers take care of hiding when appropriate.
                this.suppressNextBlurConfirm = false;
                StateHasChanged();
                return;
            }

            if (this.suggestions != null && this.suggestions.Count > 0)
            {
                // keep visible and re-position
                this.showSuggestions = true;
                StateHasChanged();
                try
                {
                    await JS.InvokeVoidAsync("userSearch.setDropdownPosition", DropdownId, InputId);
                }
                catch { }
                return;
            }

            this.showSuggestions = false;
            StateHasChanged();

            try
            {
                await JS.InvokeVoidAsync("userSearch.hideDropdownInlineStyles", DropdownId);
            }
            catch { }
        }

        public void Dispose()
        {
            try { debounceTimer?.Stop(); debounceTimer?.Dispose(); } catch { }
            CancelCurrentSearchCts();
        }
    }
}
