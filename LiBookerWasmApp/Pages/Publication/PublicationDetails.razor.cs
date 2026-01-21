using LiBooker.Shared.DTOs;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Pages.Publication
{
    public partial class PublicationDetails
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public required PublicationClient PublicationClient { get; set; }

        private PublicationMainInfo? publication;
        private bool isLoading = true;
        private string? error;

        protected override async Task OnInitializedAsync()
        {
            await LoadPublicationAsync();
        }

        private async Task LoadPublicationAsync()
        {
            this.isLoading = true;
            try
            {
                var result = await PublicationClient.GetByIdAsync(this.Id);
                if (result.IsSuccess)
                {
                    this.publication = result.Data;

                    // Specific logic for image loading if needed,
                    // though GetByIdAsync returns MainInfo which might contain image bytes/id
                    if (this.publication != null && this.publication.Image == null && this.publication.ImageId > 0)
                    {
                        // Optionally load image separately if not included in main info
                        var imgResult = await this.PublicationClient.GetImagesAsync(new List<int> { this.publication.ImageId });
                        if (imgResult.IsSuccess && imgResult.Data != null && imgResult.Data.Count > 0)
                        {
                            this.publication.Image = imgResult.Data[0].RawImage;
                        }
                    }
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
                this.isLoading = false;
            }
        }

        private string GetImageDataUrl(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(imageData);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
