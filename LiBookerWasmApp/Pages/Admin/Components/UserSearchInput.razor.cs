using LiBooker.Shared.DTOs.Admin;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LiBookerWasmApp.Pages.Admin.Components
{
    public partial class UserSearchInput : IDisposable
    {
        [Parameter] public EventCallback<string> OnSearchConfirmed { get; set; }
        [Parameter] public string Placeholder { get; set; } = "search...";

        [Inject] public required UserClient UserClient { get; set; }

        private string searchTerm = "";
        private bool showSuggestions = false;
        private List<UserManagement> suggestions = new();
        private System.Timers.Timer? debounceTimer;
        private CancellationTokenSource? searchCts;

        // suppress blur-based confirm when we already fired select or Enter
        private bool suppressNextBlurConfirm = false;

        // loading indicator for suggestions
        private bool isLoadingSuggestions = false;

        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm != value)
                {
                    searchTerm = value;
                    OnSearchInput(); // debounce-driven suggestion fetch
                }
            }
        }

        private void OnSearchInput()
        {
            // cancel previous timer
            try
            {
                debounceTimer?.Stop();
                debounceTimer?.Dispose();
            }
            catch { /* ignore */ }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // cancel any pending request too (thread-safe)
                CancelCurrentSearchCts();

                suggestions.Clear();
                showSuggestions = false;
                isLoadingSuggestions = false;
                StateHasChanged();
                return;
            }

            debounceTimer = new System.Timers.Timer(300);
            debounceTimer.AutoReset = false; // run only once
            debounceTimer.Elapsed += async (_, _) =>
            {
                await InvokeAsync(async () => await LoadSuggestionsAsync());
            };
            debounceTimer.Start();
        }

        private CancellationTokenSource SwapInNewSearchCts()
        {
            var newCts = new CancellationTokenSource();
            var prev = Interlocked.Exchange(ref searchCts, newCts);
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
            var prev = Interlocked.Exchange(ref searchCts, null);
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
            // swap in new CTS in a thread-safe manner
            var cts = SwapInNewSearchCts();
            var token = cts.Token;

            isLoadingSuggestions = true;
            StateHasChanged();

            Console.WriteLine($"[UserSearchInput] LoadSuggestionsAsync start for '{searchTerm}' at {DateTime.UtcNow:O}");

            try
            {
                var res = await UserClient.SearchUsersByEmailAsync(searchTerm, token);
                Console.WriteLine($"[UserSearchInput] API returned IsSuccess={res.IsSuccess}, IsCancelled={res.IsCancelled}");

                if (!token.IsCancellationRequested && res.IsSuccess)
                {
                    // keep suggestions small
                    suggestions = res.Data?.Take(10).ToList() ?? new List<UserManagement>();
                    showSuggestions = suggestions.Count > 0;
                    Console.WriteLine($"[UserSearchInput] suggestions count: {suggestions.Count}");
                }
                else
                {
                    // if API returned empty or cancelled, clear suggestions
                    suggestions.Clear();
                    showSuggestions = false;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[UserSearchInput] LoadSuggestionsAsync cancelled");
                suggestions.Clear();
                showSuggestions = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserSearchInput] LoadSuggestionsAsync exception: {ex}");
                suggestions.Clear();
                showSuggestions = false;
            }
            finally
            {
                isLoadingSuggestions = false;
                StateHasChanged();
                Console.WriteLine($"[UserSearchInput] LoadSuggestionsAsync finished for '{searchTerm}'");
            }
        }

        private async Task SelectSuggestion(UserManagement user)
        {
            // cancel pending suggestion requests to avoid late responses clearing UI
            CancelCurrentSearchCts();

            // prevent blur from firing an extra confirm
            suppressNextBlurConfirm = true;

            var trimmed = (user.Email ?? "").Trim();
            searchTerm = trimmed;
            showSuggestions = false;
            suggestions.Clear();

            if (OnSearchConfirmed.HasDelegate)
                await OnSearchConfirmed.InvokeAsync(trimmed);

            // small delay to make sure blur handler sees suppress flag if it runs
            await Task.Yield();
            suppressNextBlurConfirm = false;
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                // cancel pending suggestion requests to avoid late responses clearing UI
                CancelCurrentSearchCts();

                // prevent blur-based confirmation duplicate
                suppressNextBlurConfirm = true;

                showSuggestions = false;
                suggestions.Clear();

                var trimmed = (searchTerm ?? "").Trim();
                if (OnSearchConfirmed.HasDelegate)
                    await OnSearchConfirmed.InvokeAsync(trimmed);

                // small delay to reset suppression
                await Task.Yield();
                suppressNextBlurConfirm = false;
            }
        }

        private void ClearSearch()
        {
            // cancel pending ops
            try
            {
                debounceTimer?.Stop();
                debounceTimer?.Dispose();
            }
            catch { /* ignore */ }

            CancelCurrentSearchCts();

            searchTerm = "";
            suggestions.Clear();
            showSuggestions = false;

            // notify parent to clear results
            if (OnSearchConfirmed.HasDelegate)
                _ = OnSearchConfirmed.InvokeAsync("");

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

            if (suppressNextBlurConfirm)
            {
                // reset and do nothing
                suppressNextBlurConfirm = false;
                showSuggestions = false;
                StateHasChanged();
                return;
            }

            // IMPORTANT: Do NOT perform full-search on blur anymore.
            // Just hide suggestions and keep current searchTerm as-is.
            showSuggestions = false;
            StateHasChanged();
        }

        public void Dispose()
        {
            try
            {
                debounceTimer?.Stop();
                debounceTimer?.Dispose();
            }
            catch { /* ignore */ }

            CancelCurrentSearchCts();
        }
    }
}
