using LiBooker.Shared.DTOs;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Pages.Publication
{
    public partial class PublicationDetails
    {
        [Parameter]
        public int PublicationId { get; set; }
        
        [SupplyParameterFromQuery]
        public int? AvailableCopies { get; set; }

        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        [Inject]
        public required BookClient BookClient { get; set; }

        private string? error;
        private byte[]? rawPicture;
        private LiBooker.Shared.DTOs.PublicationDetails? details;
        private CancellationTokenSource? cts;

        private string BookDescription { get; set; } = string.Empty;
        private bool IsLoading { get; set; } = true;
        private bool IsPublicationAvailable { get; set; } = false;

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
            this.cts = new CancellationTokenSource();
            var token = this.cts.Token;
            try
            {
                var result = await this.PublicationClient.GetPublicationDetailsAsync(this.PublicationId, token);
                if (result.IsSuccess)
                {
                    this.details = result.Data;
                    await LoadHeavyObjects(token);
                }
                else
                {
                    this.error = result.Error ?? "Publication not found.";
                }
            }
            catch (Exception ex)
            {
                this.error = $"Error loading publication: {ex.Message}";
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private async Task LoadHeavyObjects(CancellationToken token)
        {
            if (this.details == null)
                return;

            if (this.details.PictureId.HasValue || this.details.PictureId > 0)
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
                this.BookDescription = (descResult.Data is not null) 
                    ? (descResult.Data != string.Empty) ? descResult.Data : "Description of the book is not currently available"
                    : string.Empty;
            else
                this.BookDescription = "Not found.";

            StateHasChanged();
        }
    }
}
