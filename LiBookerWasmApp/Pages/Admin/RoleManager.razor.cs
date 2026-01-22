using LiBooker.Shared.DTOs.Admin;
using LiBooker.Shared.Roles;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;

namespace LiBookerWasmApp.Pages.Admin
{
    public partial class RoleManager : IDisposable
    {
        [Inject]
        public required UserClient UserClient { get; set; }

        // state
        private string currentSearchTerm = "";
        private bool isLoading = false;
        private string? error = null;
        private CancellationTokenSource? ctsLoad;
        private CancellationTokenSource? ctsEdit;
        private List<UserManagement> allLoadedUsers = [];
        private List<UserManagement> pagedUsers = [];


        // pagination
        private int currentPage = 1;
        private const int PageSize = 8;
        private int TotalPages => (int)Math.Ceiling((double)this.allLoadedUsers.Count / PageSize);

        // modal State
        private bool isModalOpen = false;
        private UserManagement? selectedUserForEdit;
        private List<string> editAssignedRoles = [];
        private List<string> editAvailableRoles = [];

        private void OnSearchRequested(string term)
        {
            this.currentSearchTerm = term;
            _ = LoadUsersAsync(term);
        }

        private async Task LoadUsersAsync(string term)
        {
            this.ctsLoad?.Cancel();
            //this.ctsLoad?.Dispose();
            Console.WriteLine($"Takze, dostal som term '{term}' a idem nacitavat");
            // If term is empty, just clear and return
            if (string.IsNullOrWhiteSpace(term))
            {
                this.allLoadedUsers.Clear();
                UpdatePage();
                StateHasChanged(); // Ensure UI reflects clear state
                return;
            }
            Console.WriteLine("Searching fooor:"+term);
            this.ctsLoad = new CancellationTokenSource();
            var token = this.ctsLoad.Token;
            
            this.isLoading = true;
            this.error = null;
            StateHasChanged();

            try
            {
                //  fetch all matches once
                var result = await this.UserClient.SearchUsersByEmailAsync(term, token);
                
                // check token before processing result (it might have been cancelled during await)
                if (token.IsCancellationRequested) return;

                if (result.IsSuccess)
                {
                    this.allLoadedUsers = result.Data ?? [];
                    this.currentPage = 1;
                    UpdatePage();
                }
                else
                {
                    if (!result.IsCancelled) // do not treat cancellation as an API error
                    {
                        this.error = result.Error;
                    }
                    this.allLoadedUsers.Clear();
                }
            }
            catch (OperationCanceledException) // IGNORE: This is expected when user types quickly.
            {
            }
            catch (Exception ex)
            {
                this.error = ex.Message;
            }
            finally
            {   // Only update loading state if we are still the active request
                if (!token.IsCancellationRequested)
                {
                    this.isLoading = false;
                    StateHasChanged();
                }
            }
        }

        // Client-side pagination logic
        private void UpdatePage()
        {
            if (this.allLoadedUsers.Count == 0)
            {
                this.pagedUsers = [];
            }
            else
            {
                this.pagedUsers = [.. this.allLoadedUsers
                    .Skip((this.currentPage - 1) * PageSize)
                    .Take(PageSize)];
            }
        }

        private void NextPage()
        {
            if (this.currentPage < this.TotalPages)
            {
                this.currentPage++;
                UpdatePage();
            }
        }

        private void PrevPage()
        {
            if (this.currentPage > 1)
            {
                this.currentPage--;
                UpdatePage();
            }
        }

        // --- Modal Logic ---

        private void OpenEditModal(UserManagement user)
        {
            this.selectedUserForEdit = user;
            
            // get all possible existing roles
            var allSystemRoles = UserRolesExtensions.GetAllRoleNames().ToList();

            // current user roles
            this.editAssignedRoles = [.. user.Roles];

            // available = All - Assigned
            this.editAvailableRoles = [.. allSystemRoles.Where(r => !this.editAssignedRoles.Contains(r))];

            this.isModalOpen = true;
        }

        private void CloseModal()
        {
            this.isModalOpen = false;
            this.selectedUserForEdit = null;
        }

        private async Task SaveRolesAsync()
        {
            this.ctsEdit?.Cancel();
            //this.ctsEdit?.Dispose();

            if (this.selectedUserForEdit == null)
                return;

            this.ctsEdit = new CancellationTokenSource();
            var token = this.ctsEdit.Token;
            var request = new UserRolesUpdate
            {
                Email = this.selectedUserForEdit.Email,
                NewRoles = this.editAssignedRoles
            };
            try
            {
                var result = await this.UserClient.UpdateUserRolesAsync(request, token);
                if (result.IsSuccess)
                {
                    // Update local model to reflect changes immediately
                    this.selectedUserForEdit.Roles = [.. this.editAssignedRoles];
                    CloseModal();
                    StateHasChanged();
                }
                else
                {
                    Console.WriteLine($"Error saving roles: {result.Error}");
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        // --- Role Swapping Logic ---
        private void AddRole(string role)
        {
            if (this.editAvailableRoles.Contains(role))
            {
                this.editAvailableRoles.Remove(role);
                this.editAssignedRoles.Add(role);
            }
        }

        private void RemoveRole(string role)
        {
            if (this.editAssignedRoles.Contains(role))
            {
                this.editAssignedRoles.Remove(role);
                this.editAvailableRoles.Add(role);
            }
        }

        public void Dispose()
        {
            try
            {
                this.ctsLoad?.Cancel();
                this.ctsEdit?.Cancel();
                //this.ctsLoad?.Dispose();
                //this.ctsEdit?.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }
    }
}
