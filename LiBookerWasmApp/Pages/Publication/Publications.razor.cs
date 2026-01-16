using LiBooker.Shared.DTOs;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using LiBooker.Shared.EndpointParams;

namespace LiBookerWasmApp.Pages.Publication
{
    public partial class Publications : IDisposable
    {
        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        /// <summary>Load 3 imageData at a time</summary>
        private const int ImageBatchSize = 3;
        private const int pageSize = 15;

        private bool isLoading;
        private bool isLoadingImages;
        private bool isSearching = false;
        private bool showSearchResults = false;
        private int currentPage = 1;
        private int imagesLoaded;
        private int totalPublications = -1;
        private string? error = null;
        private string filterAvailability = "all"; // Filter state
        private string filterSort = "none"; // Sort state
        private string searchTerm = "";

        private System.Timers.Timer? debounceTimer;
        private List<FoundMatch> searchResults = [];
        private List<PublicationMainInfo>? publications;

        private CancellationTokenSource? publicationsCts = null;
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

        /// <summary>Max publications per page</summary>
        public static int PageSize => pageSize;

        protected override async Task OnInitializedAsync()
        {
            await LoadPublications();
        }

        private async Task OnFilterChanged()
        {
            this.currentPage = 1; // Reset to first page when changing filters to avoid empty states
            await LoadPublications();
        }

        private void PrepareForNewLoad(out CancellationToken newToken)
        {
            // cancel previous operations if running
            this.publicationsCts?.Cancel();
            this.publicationsCts?.Dispose();

            // create new token source
            this.publicationsCts = new CancellationTokenSource();
            newToken = this.publicationsCts.Token;

            // reset UI flags & state for new load
            this.isLoading = true;
            this.error = null;
            this.imagesLoaded = 0;
            this.publications = null; // clearing old list immediately to show spinner only
        }

        private async Task LoadPublications()
        {
            this.PrepareForNewLoad(out var currentToken);
            try 
            {
                // ---------------- new request ----------------
                if (this.totalPublications < 0) // only load count if not loaded yet
                    _ = LoadPublicationsCountAsync(currentToken); // fire and forget count load

                // load publications WITHOUT imageData (fast - for better UX)
                var result = await PublicationClient.GetPublicationsAsync(this.currentPage, PageSize,
                        PublicationParams.ParseAvailabilityParam(this.filterAvailability),
                        PublicationParams.ParseSortParam(this.filterSort),
                        currentToken);

                // if cancelled in the meantime, stop immediately without error
                if (currentToken.IsCancellationRequested) return;

                this.isLoading = false;

                if (result.IsSuccess)
                {
                    this.publications = result.Data;
                    StateHasChanged(); // Update UI immediately with metadata

                    // Step 2: Load imageData progressively in batches
                    if (this.publications != null && this.publications.Count > 0)
                       _ = LoadImagesProgressively(currentToken); // fire and forget with the same token
                }
                else
                {
                    // Check if this is just a cancellation
                    if (!result.IsCancelled) 
                    {
                        this.error = result.Error;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // CORRECT BEHAVIOR: Do nothing. The user started a new action, 
                // so the old one being cancelled is expected and NOT an error.
            }
            catch (Exception ex)
            {
               // Real error
               this.error = $"Unexpected error: {ex.Message}";
               this.isLoading = false;
            }
            finally
            {
                // Only turn off loading if we are NOT cancelled (if cancelled, new load is taking over)
                if (!currentToken.IsCancellationRequested)
                {
                   // StateHasChanged(); // Optional if needed here, usually handled above
                }
            }
        }

        private async Task LoadPublicationsCountAsync(CancellationToken token = default)
        {
            try
            {
                var result = await PublicationClient.GetPublicationsCountAsync(token);
                if (result.IsSuccess)
                {
                    this.totalPublications = result.Data;
                    StateHasChanged();
                }
                else if (result.IsCancelled)
                {
                    // Do nothing, operation was cancelled
                }
                else
                {
                    this.error = result.Error ?? "Unknown error";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading publications count: {ex.Message}");
            }
        }
        private async Task LoadImagesProgressively(CancellationToken ct)
        {
            if (this.publications == null || this.publications.Count == 0)
                return;
            this.isLoadingImages = true;
            StateHasChanged();
            try
            {
                // Load imageData in batches of ImageBatchSize
                for (int i = 0; i < this.publications.Count; i += ImageBatchSize)
                {
                    if (ct.IsCancellationRequested) break;

                    var batch = publications.Skip(i).Take(ImageBatchSize).ToList();
                    var ids = batch.Select(p => p.ImageId).ToList();

                    var imageResult = await PublicationClient.GetImagesAsync(ids, ct);

                    if (imageResult.IsSuccess)
                        UpdateWithNewImages(batch, imageResult.Data);
                    else
                    {
                        if (imageResult.IsCancelled)
                            return; // do nothing
                        this.error = imageResult.Error ?? "Unknown error";
                    }
                    await Task.Delay(100, ct); // Small delay between batches to avoid overwhelming the server
                }
            }
            catch (OperationCanceledException)
            {
                // User navigated away or cancelled
            }
            finally
            {
                this.isLoadingImages = false;
                StateHasChanged();
            }
        }

        private void UpdateWithNewImages(List<PublicationMainInfo> batch, List<PublicationImage>? imageData)
        {
            if (imageData == null || imageData.Count == 0)
                return;
            var pairs = imageData.ToDictionary(data => data.ImageId, data => data.RawImage);
            //Console.WriteLine($"Loaded batch of {pairs.Count} imageData.");
            // Update publications with loaded imageData
            foreach (var pub in batch)
            {
                if (pairs.TryGetValue(pub.ImageId, out var imageBytes))
                {
                    pub.Image = imageBytes;
                    this.imagesLoaded++;
                }
            }
            StateHasChanged(); // Update UI after each batch
        }

        private async Task NextPage()
        {
            this.currentPage++;
            await LoadPublications();
        }

        private async Task PreviousPage()
        {
            if (this.currentPage > 1)
            {
                this.currentPage--;
                await LoadPublications();
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

        private async Task PerformSearch(string searchTerm) // Assuming you have logic here
        {
            // Cancel previous search
            this.searchCts?.Cancel();
            this.searchCts?.Dispose();
            this.searchCts = new CancellationTokenSource();
            var token = this.searchCts.Token;

            // set flags
            this.isSearching = true;

            try
            {
                // Debounce simple implementation (wait for typing to stop)
                await Task.Delay(300, token); 

                var result = await PublicationClient.GetAllSearchMatchesAsync(searchTerm, token);

                if (!token.IsCancellationRequested && result.IsSuccess)
                {
                    this.searchResults = result.Data ?? [];
                    this.showSearchResults = true;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignored - superseded by new character typed
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
        
        // Using onmousedown instead of onclick because blur happens before click
        private void SelectSearchResult(int publicationId)
        {
            // Navigate to details or filter the main grid
            // NavigationManager.NavigateTo($"/publications/{publicationId}");
            Console.WriteLine($"Selected: {publicationId}");
        }

        private void SelectSearchResultItem(FoundMatch match)
        {
            if (match is BookMatch book)
            {
                //Console.WriteLine($"Selected Book: {book.Title}");
                //NavigationManager.NavigateTo($"/publication/{book.Id}");
            }
            else if (match is AuthorMatch author)
            {
                 // Logika pre autora (napr. nastavenie filtra do searchbaru)
                 this.SearchTerm = author.FullName;
                 // PerformSearch(); // spustiť vyhľadávanie pre autora
            }
            else if (match is GenreMatch genre)
            {
                 this.SearchTerm = genre.Name;
            }

            this.showSearchResults = false;
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
            // Optionally reload original list or focus input here
            StateHasChanged();
        }

        public void Dispose()
        {
            // Dispose ALL cancellation sources
            this.publicationsCts?.Cancel();
            this.publicationsCts?.Dispose();

            this.searchCts?.Cancel();
            this.searchCts?.Dispose();
        }

        private static string GetImageDataUrl(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(imageData);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
