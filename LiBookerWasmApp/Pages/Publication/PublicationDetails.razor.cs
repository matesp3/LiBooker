using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Pages.Publication
{
    public partial class PublicationDetails : IDisposable
    {
        [Parameter]
        public int PublicationId { get; set; }
        
        [SupplyParameterFromQuery]
        public int? AvailableCopies { get; set; }

        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        [Inject]
        public required BookClient BookClient { get; set; }

        // Backing model(s) used by Razor markup.
        // Keep both names to match existing Razor references: `details` and `publication`.
        private LiBooker.Shared.DTOs.PublicationDetails? details;
        private LiBooker.Shared.DTOs.PublicationDetails? publication;

        private string? error;
        private byte[]? rawPicture;
        private CancellationTokenSource? cts;

        private string BookDescription { get; set; } = string.Empty;
        private bool IsLoading { get; set; } = true;
        private bool IsPublicationAvailable { get; set; } = false;

        // Modal state for reservation dialog
        private bool isReserveModalOpen = false;

        protected override async Task OnInitializedAsync()
        {
            if (this.AvailableCopies.HasValue)
            {
               this.IsPublicationAvailable = this.AvailableCopies.Value > 0;
            }

            await LoadPublicationAsync();
        }

        private async Task LoadPublicationAsync()
        {
            this.IsLoading = true;
            this.cts?.Cancel();
            try
            {
                this.cts = new CancellationTokenSource();
                var token = this.cts.Token;
                var result = await this.PublicationClient.GetPublicationDetailsAsync(this.PublicationId, token);
                if (result.IsSuccess)
                {
                    // set both names used in razor to avoid mismatch issues
                    this.details = result.Data;
                    this.publication = result.Data;

                    // If AvailableCopies was not supplied via query, try to infer availability from server data (if available)
                    // Otherwise keep earlier IsPublicationAvailable based on query parameter.
                    // (If API returns availability info, update logic here.)
                    if (!this.AvailableCopies.HasValue)
                    {
                        // fallback: assume available if details is not null (you can refine)
                        this.IsPublicationAvailable = true;
                    }

                    // load heavy objects (image/description)
                    await LoadHeavyObjects(token);
                }
                else
                {
                    this.error = result.Error ?? "Publication not found.";
                    this.details = null;
                    this.publication = null;
                }
            }
            catch (OperationCanceledException)
            {
                // cancelled - ignore
            }
            catch (Exception ex)
            {
                this.error = $"Error loading publication: {ex.Message}";
                this.details = null;
                this.publication = null;
            }
            finally
            {
                this.IsLoading = false;
                StateHasChanged();
            }
        }

        private async Task LoadHeavyObjects(CancellationToken token)
        {
            if (this.details == null)
                return;
            try
            {
                if (this.details.PictureId.HasValue && this.details.PictureId.Value > 0)
                {
                    int picId = this.details.PictureId.Value;
                    var imgResult = await this.PublicationClient.GetImagesAsync([picId], token);
                    if (imgResult.IsSuccess && imgResult.Data != null && imgResult.Data.Count > 0)
                        this.rawPicture = imgResult.Data[0].RawImage;
                    else
                        this.rawPicture = null;
                }
                var descResult = await this.BookClient.GetBookDescriptionAsync(this.PublicationId, token);

                if (descResult.IsSuccess)
                    this.BookDescription = (descResult.Data is not null && descResult.Data != string.Empty)
                        ? descResult.Data
                        : "Description of the book is not currently available";
                else
                    this.BookDescription = "Not found.";

                StateHasChanged();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        // Called from UI to open reservation modal
        private void OpenReserveModal()
        {
            // ensure we have publication loaded
            if (this.publication == null)
            {
                // try reload synchronously in background
                _ = LoadPublicationAsync();
            }
            isReserveModalOpen = true;
        }

        // Handler called when reservation is successfully created in modal
        private async Task OnReservationCreated()
        {
            // close modal
            isReserveModalOpen = false;

            // refresh publication details to reflect availability changes
            await LoadPublicationAsync();
        }

        // Expose publication for binding in Razor (AddReservationModal Publication="@publication")
        // If Razor expects a property, it will use the field; fields are fine.
        // Provide a read-only accessor as well for clarity:
        private LiBooker.Shared.DTOs.PublicationDetails? PublicationModel => this.publication;

        public void Dispose()
        {
            try { this.cts?.Cancel(); } catch { }
            try { this.cts?.Dispose(); } catch { }
        }
    }
}
