using LiBooker.Shared.DTOs;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LiBookerWasmApp.Layout
{
    public partial class GlobalSearch : IDisposable
    {
        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject] // for JS interop
        public required IJSRuntime JSRuntime { get; set; }

        private bool isSearching = false;
        private bool showSearchResults = false;
        private string searchTerm = "";
        private System.Timers.Timer? debounceTimer;
        private List<FoundMatch> searchResults = [];
        private CancellationTokenSource? searchCts = null;
        private int selectedIndex = -1;

        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                searchTerm = value;
                OnSearchInput(); // Trigger debounce on every keystroke
            }
        }

        private void OnSearchInput()
        {
            // Reset timer
            this.debounceTimer?.Stop();
            this.debounceTimer?.Dispose();

            if (string.IsNullOrWhiteSpace(this.searchTerm))
            {
                this.searchResults.Clear();
                this.showSearchResults = false;
                return;
            }

            this.showSearchResults = true;
            this.isSearching = true;

            // Set new timer for 300ms
            this.debounceTimer = new System.Timers.Timer(300);
            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += async (sender, args) =>
            {
                await InvokeAsync(async () => await PerformSearch(this.searchTerm));
            };
            this.debounceTimer.Start();
        }

        private async Task PerformSearch(string searchTerm)
        {
            // Cancel previous search
            this.searchCts?.Cancel();
            this.searchCts?.Dispose();
            this.searchCts = new CancellationTokenSource();
            var token = this.searchCts.Token;

            this.isSearching = true;
            StateHasChanged();

            try
            {
                var result = await PublicationClient.GetAllSearchMatchesAsync(searchTerm, token);

                if (!token.IsCancellationRequested && result.IsSuccess)
                {
                    this.searchResults = result.Data ?? [];
                    this.showSearchResults = true;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignored - superseded
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    this.isSearching = false;
                    StateHasChanged();
                }
            }

            this.selectedIndex = -1; // Reset selection
        }

        private void SelectSearchResultItem(FoundMatch match)
        {
            this.showSearchResults = false;
            this.SearchTerm = string.Empty; // Optionally clear search after navigation
            
            if (match is BookMatch book)
            {
                // Navigate to book details
                // NavigationManager.NavigateTo($"/publication/{book.Id}");
                Console.WriteLine($"Navigate to book: {book.Title} ({book.Id})");
            }
            else if (match is AuthorMatch author)
            {
                // Navigate to publications filtered by author (logic to be implemented on Publications page)
                 Console.WriteLine($"Filter by author: {author.FullName}");
                 // For now, we can just navigate to publications, you might want to add query strings later
                 this.NavigationManager.NavigateTo("publications"); 
            }
            else if (match is GenreMatch genre)
            {
                 Console.WriteLine($"Filter by genre: {genre.Name}");
                 this.NavigationManager.NavigateTo("publications");
            }
        }

        // Helper for highlighting text
        private MarkupString HighlightText(string text)
        {
            if (string.IsNullOrEmpty(SearchTerm) || string.IsNullOrEmpty(text))
                return new MarkupString(text);

            var pattern = System.Text.RegularExpressions.Regex.Escape(this.SearchTerm);
            var result = System.Text.RegularExpressions.Regex.Replace(text, $"({pattern})", "<strong>$1</strong>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return new MarkupString(result);
        }

        // Initialize shortcut on first render
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Calls the JS function to attach the keydown event listener
                await this.JSRuntime.InvokeVoidAsync("liBookerSearch.initializeGlobalSearchShortcut", "globalSearchInput");
            }
        }

        // HandleKeyDown to trigger scrolling
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            // 1. Handle Escape key FIRST
            if (e.Key == "Escape")
            {
                this.showSearchResults = false;
                // Call JS to remove focus from the input
                await this.JSRuntime.InvokeVoidAsync("liBookerSearch.blurElement", "globalSearchInput");
                return;
            }

            var allResults = this.searchResults.Cast<FoundMatch>().ToList();

            if (allResults.Count == 0) return;

            if (e.Key == "ArrowDown")
            {
                this.selectedIndex = Math.Min(this.selectedIndex + 1, allResults.Count - 1);
                await ScrollToActive(); // Call JS to scroll to the active item
            }
            else if (e.Key == "ArrowUp")
            {
                this.selectedIndex = Math.Max(this.selectedIndex - 1, 0);
                await ScrollToActive(); // Call JS to scroll to the active item
            }
            else if (e.Key == "Enter" && this.selectedIndex >= 0)
            {
                SelectSearchResultItem(allResults[this.selectedIndex]);
            }
        }

        // Helper to invoke JS scrolling
        private async Task ScrollToActive()
        {
            // We need to wait for the UI to update the "active" class first
            // StateHasChanged isn't always instant in terms of DOM repaint for JS to see
            StateHasChanged(); 
            // Small delay to ensure DOM has the .active class applied before JS runs
            await Task.Yield(); 
            await this.JSRuntime.InvokeVoidAsync("liBookerSearch.scrollToActiveItem", "searchResultsDropdown");
        }

        private async Task OnSearchBlur()
        {
            // Give time for click event to register before hiding
            await Task.Delay(200);
            this.showSearchResults = false;
        }

        private void ClearSearch()
        {
            this.SearchTerm = string.Empty;
            this.showSearchResults = false;
            this.searchResults.Clear();
        }

        public void Dispose()
        {
            this.debounceTimer?.Stop();
            this.debounceTimer?.Dispose();
            this.searchCts?.Cancel();
            this.searchCts?.Dispose();
        }
    }
}