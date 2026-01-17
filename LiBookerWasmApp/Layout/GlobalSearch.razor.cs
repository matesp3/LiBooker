using LiBooker.Shared.DTOs;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Layout
{
    public partial class GlobalSearch : IDisposable
    {
        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        private bool isSearching = false;
        private bool showSearchResults = false;
        private string searchTerm = "";
        private System.Timers.Timer? debounceTimer;
        private List<FoundMatch> searchResults = [];
        private CancellationTokenSource? searchCts = null;

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
                 NavigationManager.NavigateTo("publications"); 
            }
            else if (match is GenreMatch genre)
            {
                 Console.WriteLine($"Filter by genre: {genre.Name}");
                 NavigationManager.NavigateTo("publications");
            }
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