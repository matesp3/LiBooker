using LiBooker.Shared.DTOs;
using LiBooker.Shared.DTOs.Admin;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Pages.Publication.Components
{
    public partial class AddReservationModal : ComponentBase, IDisposable
    {
        [Parameter] public LiBooker.Shared.DTOs.PublicationDetails? Publication { get; set; }
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public EventCallback OnReservationSuccess { get; set; }

        [Inject] public required LoanClient LoanClient { get; set; }

        // component state
        private UserManagement? selectedUser;
        private bool isSaving = false;
        private string? error;

        private bool IsAddDisabled => selectedUser == null || selectedUser.PersonId == null || isSaving;

        // Called by UserSearchInput when an item is selected
        private void HandleUserSelected(UserManagement user)
        {
            selectedUser = user;
            error = null;
            StateHasChanged();
        }

        // Create reservation request
        private async Task CreateReservationAsync()
        {
            if (Publication == null)
            {
                error = "Publication not provided.";
                StateHasChanged();
                return;
            }

            if (selectedUser?.PersonId == null)
            {
                error = "Please select a user with a valid PersonId.";
                StateHasChanged();
                return;
            }

            isSaving = true;
            error = null;
            StateHasChanged();

            try
            {
                var request = new LoanRequest
                {
                    PersonId = selectedUser.PersonId.Value,
                    PublicationId = Publication.PublicationId
                };

                var resp = await LoanClient.CreateLoanRequestAsync(request);
                if (resp.IsSuccess)
                {
                    // success: close modal and notify parent
                    await CloseModal();
                    if (OnReservationSuccess.HasDelegate)
                        await OnReservationSuccess.InvokeAsync();
                }
                else
                {
                    error = resp.Error ?? "Failed to create reservation.";
                }
            }
            catch (Exception ex)
            {
                error = $"An error occurred: {ex.Message}";
            }
            finally
            {
                isSaving = false;
                StateHasChanged();
            }
        }

        // Close modal and reset state
        private async Task CloseModal()
        {
            // reset internal state
            selectedUser = null;
            error = null;
            isSaving = false;

            IsOpen = false;
            await IsOpenChanged.InvokeAsync(false);
            StateHasChanged();
        }

        // Dispose pattern (no resources currently but keep for extensibility)
        public void Dispose()
        {
            // nothing to dispose currently
        }
    }
}
