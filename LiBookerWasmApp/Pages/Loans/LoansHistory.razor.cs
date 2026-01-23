using LiBooker.Shared.DTOs;
using LiBooker.Shared.DTOs.Admin;
using LiBooker.Shared.Roles;
using LiBookerWasmApp.Services.Clients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace LiBookerWasmApp.Pages.Loans
{
    public partial class LoansHistory : IDisposable
    {
        [Inject] public required LoanClient LoanClient { get; set; }
        [Inject] public required AuthenticationStateProvider AuthStateProvider { get; set; }

        // State
        private bool canManage = false;
        private int? currentPersonId; // PersonId whose history we are viewing
        private string? currentPersonEmail; // Email for the header display
        private List<LoanInfo> allLoans = [];
        private List<LoanInfo> pagedLoans = [];
        private bool isLoading = false;
        private string? errorMessage;

        // Pagination
        private int currentPage = 1;
        private const int PageSize = 7;
        private int TotalPages => (int)Math.Ceiling((double)this.allLoans.Count / PageSize);

        // Modal State
        private bool showReturnModal = false;
        private LoanInfo? loanToReturn;
        private bool isProcessingReturn = false;

        private CancellationTokenSource? cts; // used for loading
        private CancellationTokenSource? ctsReturn; // used for return operation

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            this.canManage = user.IsInRole(UserRolesExtensions.GetRoleName(UserRoles.Admin))
                            || user.IsInRole(UserRolesExtensions.GetRoleName(UserRoles.Librarian));

            if (!this.canManage)
            {
                // Regular USER: Load own history immediately
                var personIdClaim = user.FindFirst("PersonId");
                if (int.TryParse(personIdClaim?.Value, out int pid))
                {
                    this.currentPersonId = pid;
                    this.currentPersonEmail = user.Identity?.Name ?? user.FindFirst(ClaimTypes.Email)?.Value;
                    await LoadLoansAsync(pid);
                }
            }
        }

        // --- Event Handlers ---

        /// <summary>
        /// Handler for UserSearchInput (Admin only).
        /// </summary>
        private async Task HandleUserSelected(UserManagement user)
        {
            if (user.PersonId.HasValue)
            {
                this.currentPersonId = user.PersonId.Value;
                this.currentPersonEmail = user.Email;
                this.currentPage = 1; // reset pagination
                await LoadLoansAsync(this.currentPersonId.Value);
            }
            else
            {
                this.allLoans.Clear();
                UpdatePage();
                this.errorMessage = "Selected user has no linked Person ID.";
            }
        }

        private async Task LoadLoansAsync(int personId)
        {
            this.cts?.Cancel();
            try { this.cts?.Dispose(); } catch { }
            this.cts = new CancellationTokenSource();
            var token = this.cts.Token;

            this.isLoading = true;
            this.errorMessage = null;
            StateHasChanged();

            try
            {
                var response = await this.LoanClient.GetLoansForPersonAsync(personId, token);
                if (response.IsSuccess && response.Data != null)
                {
                    this.allLoans = response.Data
                        .OrderByDescending(x => x.LoanId)
                        .ToList();
                }
                else
                {
                    this.errorMessage = response.Error ?? "Failed to load loans.";
                    this.allLoans.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                // load cancelled - keep state as-is or clear as desired
            }
            catch (Exception ex)
            {
                this.errorMessage = ex.Message;
                this.allLoans.Clear();
            }
            finally
            {
                this.isLoading = false;
                UpdatePage();
                StateHasChanged();
            }
        }

        // --- Return Logic ---

        private void RequestReturn(LoanInfo loan)
        {
            this.loanToReturn = loan;
            this.showReturnModal = true;
        }

        private void CloseModal()
        {
            this.showReturnModal = false;
            this.loanToReturn = null;
        }

        private async Task ConfirmReturnAsync()
        {
            if (this.loanToReturn == null) return;

            if (this.isProcessingReturn) return; // prevent concurrent confirms

            this.isProcessingReturn = true;
            StateHasChanged();

            // Cancel previous return if any, then create new CTS for this operation
            try
            {
                this.ctsReturn?.Cancel();
            }
            catch { /* ignore */ }
            try
            {
                this.ctsReturn?.Dispose();
            }
            catch { /* ignore */ }

            this.ctsReturn = new CancellationTokenSource();
            var token = this.ctsReturn.Token;

            try
            {
                await UpdateLoanAsync(token);
            }
            catch (OperationCanceledException)
            {
                // cancelled
                this.errorMessage = "Return operation was cancelled.";
            }
            catch (Exception ex)
            {
                this.errorMessage = ex.Message;
            }
            finally
            {
                this.isProcessingReturn = false;
                try
                {
                    this.ctsReturn?.Dispose();
                }
                catch { }
                this.ctsReturn = null;
                StateHasChanged();
            }
        }

        private async Task UpdateLoanAsync(CancellationToken token)
        {
            if (this.loanToReturn == null) return;
            // Prepare updated DTO with ReturnDate set to Now
            var updatedLoan = new LoanInfo
            {
                LoanId = this.loanToReturn.LoanId,
                PublicationId = this.loanToReturn.PublicationId,
                BookTitle = this.loanToReturn.BookTitle,
                DateFrom = this.loanToReturn.DateFrom,
                DateTo = this.loanToReturn.DateTo,
                ReturnDate = DateTime.Now
            };

            var response = await this.LoanClient.UpdateLoanAsync(updatedLoan, token);

            if (!token.IsCancellationRequested && response.IsSuccess)
            {
                // Update local state to reflect change without full reload
                var local = this.allLoans.FirstOrDefault(l => l.LoanId == this.loanToReturn.LoanId);
                if (local != null)
                {
                    local.ReturnDate = updatedLoan.ReturnDate;
                }
                UpdatePage(); // refresh current view
                CloseModal();
            }
            else if (token.IsCancellationRequested)
            {
                this.errorMessage = "Return operation was cancelled.";
            }
            else
            {
                // show error (response failed)
                this.errorMessage = response.Error ?? "Failed to return publication.";
                CloseModal();
            }
        }

        // --- Pagination ---

        private void UpdatePage()
        {
            if (this.allLoans.Count == 0)
            {
                this.pagedLoans = [];
            }
            else
            {
                this.pagedLoans = this.allLoans
                    .Skip((this.currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
        }

        private void PrevPage()
        {
            if (this.currentPage > 1) { this.currentPage--; UpdatePage(); }
        }

        private void NextPage()
        {
            if (this.currentPage < this.TotalPages) { this.currentPage++; UpdatePage(); }
        }

        public void Dispose()
        {
            try
            {
                this.cts?.Cancel();
            }
            catch { }
            try
            {
                this.cts?.Dispose();
            }
            catch { }
            this.cts = null;

            try
            {
                this.ctsReturn?.Cancel();
            }
            catch { }
            try
            {
                this.ctsReturn?.Dispose();
            }
            catch { }
            this.ctsReturn = null;
        }
    }
}
