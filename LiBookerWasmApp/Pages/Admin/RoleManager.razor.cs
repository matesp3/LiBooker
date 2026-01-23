using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs.Admin;
using LiBooker.Shared.Roles;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace LiBookerWasmApp.Pages.Admin
{
    public partial class RoleManager : IDisposable
    {
        [Inject]
        public required UserClient UserClient { get; set; }
        [Inject]
        public required AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        // state
        private string currentSearchTerm = "";
        private bool isLoading = false;
        private string? error = null;
        private CancellationTokenSource? ctsLoad;
        private CancellationTokenSource? ctsEdit;
        private List<UserManagement> allLoadedUsers = [];
        private List<UserManagement> pagedUsers = [];

        // request versioning to avoid stale responses overwriting state
        private int loadRequestCounter = 0;
        private int latestActiveRequestId = 0;

        // saving/modal state
        private bool isSaving = false;
        private string? modalError = null;

        // pagination
        private int currentPage = 1;
        private const int PageSize = 8;
        private int TotalPages => (int)Math.Ceiling((double)this.allLoadedUsers.Count / PageSize);

        // modal State
        private bool isModalOpen = false;
        private UserManagement? selectedUserForEdit;
        private List<string> editAssignedRoles = [];
        private List<string> editAvailableRoles = [];

        // selection state (roles selected in UI, moved only after pressing arrows)
        private HashSet<string> selectedAvailable = new();
        private HashSet<string> selectedAssigned = new();

        private void OnSearchRequested(string term)
        {
            this.currentSearchTerm = term;
            _ = LoadUsersAsync(term);
        }

        private async Task LoadUsersAsync(string term)
        {
            // normalize input
            var trimmed = (term ?? "").Trim();

            // If term is empty, just clear and return
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                // cancel any pending load
                CancelLoadCts();
                this.allLoadedUsers.Clear();
                UpdatePage();
                this.isLoading = false;
                StateHasChanged(); // Ensure UI reflects clear state
                Console.WriteLine("[RoleManager] cleared users because term is empty");
                return;
            }

            // prepare cancellation and request id
            CancelLoadCts(); // cancel previous load
            this.ctsLoad = new CancellationTokenSource();
            var token = this.ctsLoad.Token;

            var requestId = Interlocked.Increment(ref this.loadRequestCounter);
            // mark this as the active request
            Interlocked.Exchange(ref this.latestActiveRequestId, requestId);

            Console.WriteLine($"[RoleManager] LoadUsersAsync start (id={requestId}) for '{trimmed}' at {DateTime.UtcNow:O}");

            // set loading state for the currently active request
            this.isLoading = true;
            this.error = null;
            StateHasChanged();

            try
            {
                var result = await this.UserClient.SearchUsersByEmailAsync(trimmed, token);
                Console.WriteLine($"[RoleManager] (id={requestId}) API returned IsSuccess={result.IsSuccess}, IsCancelled={result.IsCancelled}");

                // If this request is no longer the latest, ignore its result
                if (requestId != this.latestActiveRequestId)
                {
                    Console.WriteLine($"[RoleManager] (id={requestId}) Stale response — ignoring.");
                    return;
                }

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"[RoleManager] (id={requestId}) request cancelled after await");
                    return;
                }

                if (result.IsSuccess)
                {
                    this.allLoadedUsers = result.Data ?? [];
                    this.currentPage = 1;
                    UpdatePage();
                    Console.WriteLine($"[RoleManager] (id={requestId}) loaded {this.allLoadedUsers.Count} users");
                }
                else
                {
                    if (!result.IsCancelled)
                    {
                        this.error = result.Error;
                        Console.WriteLine($"[RoleManager] (id={requestId}) API error: {result.Error}");
                    }
                    this.allLoadedUsers.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[RoleManager] (id={requestId}) OperationCanceledException - ignored");
            }
            catch (Exception ex)
            {
                // Only show error if this is still the active request
                if (requestId == this.latestActiveRequestId)
                {
                    this.error = ex.Message;
                    Console.WriteLine($"[RoleManager] (id={requestId}) Exception: {ex}");
                }
                else
                {
                    Console.WriteLine($"[RoleManager] (id={requestId}) Exception on stale request: {ex}");
                }
            }
            finally
            {
                // Only turn off loading if this is the active request
                if (requestId == this.latestActiveRequestId && !token.IsCancellationRequested)
                {
                    this.isLoading = false;
                    StateHasChanged();
                    Console.WriteLine($"[RoleManager] (id={requestId}) LoadUsersAsync finished normally");
                }
            }
        }

        private void CancelLoadCts()
        {
            try
            {
                this.ctsLoad?.Cancel();
            }
            catch { }
            try
            {
                this.ctsLoad?.Dispose();
            }
            catch { }
            this.ctsLoad = null;
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

            // reset selections and modal state
            this.selectedAvailable.Clear();
            this.selectedAssigned.Clear();
            this.modalError = null;
            this.isSaving = false;

            this.isModalOpen = true;
        }

        private void CloseModal()
        {
            this.isModalOpen = false;
            this.selectedUserForEdit = null;
            this.selectedAvailable.Clear();
            this.selectedAssigned.Clear();
            this.modalError = null;
            this.isSaving = false;
        }

        private void CloseModalOnBackdrop()
        {
            // same as Cancel behavior
            CloseModal();
        }

        private async Task SaveRolesAsync()
        {
            // prevent concurrent saves
            if (this.isSaving) return;

            this.ctsEdit?.Cancel();
            this.ctsEdit?.Dispose();

            if (this.selectedUserForEdit == null)
                return;

            this.ctsEdit = new CancellationTokenSource();
            var token = this.ctsEdit.Token;

            var request = new UserRolesUpdate
            {
                UserId = this.selectedUserForEdit.UserId,
                Email = this.selectedUserForEdit.Email,
                NewRoles = this.editAssignedRoles
            };

            this.isSaving = true;
            this.modalError = null;
            StateHasChanged();

            try
            {
                var result = await this.UserClient.UpdateUserRolesAsync(request, token);

                if (token.IsCancellationRequested) // cancelled by user/navigation; do nothing
                {
                    this.isSaving = false;
                    return;
                }
                await ProcessRoleChangeResponseAsync(result);
            }
            catch (OperationCanceledException)
            {
                // treat as cancelled
                this.isSaving = false;
            }
            catch (Exception ex)
            {
                this.modalError = $"Unexpected error: {ex.Message}";
                this.isSaving = false;
                StateHasChanged();
            }
        }

        private async Task ProcessRoleChangeResponseAsync(ApiResponse<UserRolesUpdate> result)
        {
            if (result.IsSuccess)
            {
                // Update local model to reflect changes immediately
                this.selectedUserForEdit!.Roles = [.. this.editAssignedRoles];
                var selectedEmail = this.selectedUserForEdit.Email;
                CloseModal();

                var state = await this.AuthStateProvider.GetAuthenticationStateAsync();
                var user = state.User;
                (var email, var isAdmin) = GetUserState(user);

                bool redirect = false;
                if (string.Equals(email, selectedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    // If the edited user is the current user, refresh auth state
                    if (this.AuthStateProvider is Services.Auth.CustomAuthStateProvider customAuthProvider)
                    {
                        await customAuthProvider.RefreshUserAsync();
                        redirect = !isAdmin; // was in admin role, but now is OR is not?
                    }
                }
                if (redirect)
                    this.NavigationManager.NavigateTo("/");
                else
                    StateHasChanged();
            }
            else
            {
                // show inline error and keep modal open
                this.modalError = result.Error ?? "Failed to save roles.";
                this.isSaving = false;
                StateHasChanged();
            }
        }

        // --- Selection & Move Logic ---
        private void ToggleSelectAvailable(string role)
        {
            if (this.selectedAvailable.Contains(role))
                this.selectedAvailable.Remove(role);
            else
                this.selectedAvailable.Add(role);

            // clear opposite selection to avoid confusion
            this.selectedAssigned.Clear();
        }

        private void ToggleSelectAssigned(string role)
        {
            if (this.selectedAssigned.Contains(role))
                this.selectedAssigned.Remove(role);
            else
                this.selectedAssigned.Add(role);

            // clear opposite selection to avoid confusion
            this.selectedAvailable.Clear();
        }

        private void MoveSelectedToAssigned()
        {
            if (this.selectedAvailable.Count == 0) return;

            // move items
            var toMove = this.selectedAvailable.ToList();
            foreach (var r in toMove)
            {
                if (!this.editAssignedRoles.Contains(r))
                {
                    this.editAssignedRoles.Add(r);
                    this.editAvailableRoles.Remove(r);
                }
            }

            // clear selection
            this.selectedAvailable.Clear();
            StateHasChanged();
        }

        private void MoveSelectedToAvailable()
        {
            if (this.selectedAssigned.Count == 0) return;

            var toMove = this.selectedAssigned.ToList();
            foreach (var r in toMove)
            {
                if (!this.editAvailableRoles.Contains(r))
                {
                    this.editAvailableRoles.Add(r);
                    this.editAssignedRoles.Remove(r);
                }
            }

            // clear selection
            this.selectedAssigned.Clear();
            StateHasChanged();
        }

        // --- Single-item double-click moves ---
        private void MoveSingleAvailableToAssigned(string role)
        {
            if (string.IsNullOrEmpty(role)) return;
            if (!this.editAvailableRoles.Contains(role)) return;

            this.editAvailableRoles.Remove(role);
            if (!this.editAssignedRoles.Contains(role))
                this.editAssignedRoles.Add(role);

            // clear selection and refresh
            this.selectedAvailable.Clear();
            StateHasChanged();
        }

        private void MoveSingleAssignedToAvailable(string role)
        {
            if (string.IsNullOrEmpty(role)) return;
            if (!this.editAssignedRoles.Contains(role)) return;

            this.editAssignedRoles.Remove(role);
            if (!this.editAvailableRoles.Contains(role))
                this.editAvailableRoles.Add(role);

            // clear selection and refresh
            this.selectedAssigned.Clear();
            StateHasChanged();
        }

        // Adds dismiss action for inline modal error
        private void DismissModalError()
        {
            this.modalError = null;
            StateHasChanged();
        }

        public void Dispose()
        {
            try
            {
                this.ctsLoad?.Cancel();
                this.ctsEdit?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private static (string email, bool isAdmin) GetUserState(ClaimsPrincipal user)
        {
            // Email: try ClaimTypes.Email, then "email", then fallback to Name
            string? email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value
                        ?? user.Identity?.Name;

            // Is in Admin role (use existing helper for the canonical role name)
            var adminRoleName = UserRolesExtensions.GetRoleName(UserRoles.Admin);
            bool isAdmin = user.IsInRole(adminRoleName)
                           || user.HasClaim(c =>
                                (c.Type == ClaimTypes.Role
                                 || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase))
                                && string.Equals(c.Value, adminRoleName, StringComparison.OrdinalIgnoreCase));
            return (email ?? string.Empty, isAdmin);
        }
    }
}
