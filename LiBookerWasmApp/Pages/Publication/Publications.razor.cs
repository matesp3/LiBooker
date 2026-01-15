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

        private const int ImageBatchSize = 3; // Load 3 images at a time
        private bool isLoading;
        private bool isLoadingImages;
        private int currentPage = 1;
        private int imagesLoaded;
        /// <summary>
        /// Max publications per page
        /// </summary>
        private int pageSize = 15;
        private int totalPublications = 0;
        private string? error = null;
        private string filterAvailability = "all"; // Filter state
        private string filterSort = "none"; // Sort state
        private CancellationTokenSource? cts = null;
        private List<PublicationMainInfo>? publications;

        protected override async Task OnInitializedAsync()
        {
            await LoadPublications();
        }

        private async Task OnFilterChanged()
        {
            this.currentPage = 1; // Reset to first page when changing filters to avoid empty states
            await LoadPublications();
        }

        private async Task LoadPublications()
        {
            this.cts?.Cancel(); // if there is an ongoing load, cancel it
            this.cts?.Dispose();
            // ---------------- reset ----------------
            this.cts = new CancellationTokenSource();
            this.isLoading = true;
            this.error = null;
            this.imagesLoaded = 0;
            // ---------------- new request ----------------
            // Step 1: Load publications WITHOUT images (fast - for better UX)

            var result = await PublicationClient.GetAllAsync(this.currentPage, this.pageSize,
                PublicationParams.ParseAvailabilityParam(this.filterAvailability),
                PublicationParams.ParseSortParam(this.filterSort),
                this.cts.Token);

            this.isLoading = false;

            if (result.IsSuccess)
            {
                this.publications = result.Data;
                
                StateHasChanged(); // Update UI immediately with metadata

                this.totalPublications = await PublicationClient.GetPublicationsCountAsync(this.cts.Token);
                StateHasChanged();

                // Step 2: Load images progressively in batches
                if (this.publications != null && this.publications.Count > 0)
                {
                    _ = LoadImagesProgressively(this.cts.Token); // Fire and forget
                }
            }
            else
            {
                this.error = result.Error ?? "Unknown error";
            }
        }
        private async Task LoadImagesProgressively(CancellationToken ct)
        {
            if (this.publications == null || this.publications.Count == 0) return;

            this.isLoadingImages = true;
            StateHasChanged();

            try
            {
                // Load images in batches of ImageBatchSize
                for (int i = 0; i < this.publications.Count; i += ImageBatchSize)
                {
                    if (ct.IsCancellationRequested) break;

                    var batch = publications.Skip(i).Take(ImageBatchSize).ToList();
                    var ids = batch.Select(p => p.ImageId).ToList();

                    var imageResult = await PublicationClient.GetImagesAsync(ids, ct);

                    if (imageResult.IsSuccess && imageResult.Data != null)
                    {
                        var pairs = imageResult.Data.ToDictionary(data => data.ImageId, data => data.ImageData);
                        Console.WriteLine($"Loaded batch of {pairs.Count} images.");
                        // Update publications with loaded images
                        foreach (var pub in batch)
                        {
                            if (pairs.TryGetValue(pub.ImageId, out var imageData))
                            {
                                pub.Image = imageData;
                                this.imagesLoaded++;
                            }
                        }
                        StateHasChanged(); // Update UI after each batch
                    }

                    // Small delay between batches to avoid overwhelming the server
                    await Task.Delay(100, ct);
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

        private string GetImageDataUrl(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(imageData);
            return $"data:image/jpeg;base64,{base64}";
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

        public void Dispose()
        {
            this.cts?.Cancel();
            this.cts?.Dispose();
        }
    }
}
