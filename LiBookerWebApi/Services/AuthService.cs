using LiBooker.Shared.DTOs;
using LiBookerWebApi.Endpoints.ResultWrappers;
using LiBookerWebApi.Model;
using LiBookerWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiBookerWebApi.Services
{
    public class AuthService(LiBookerDbContext db) : IAuthService
    {
        private readonly LiBookerDbContext db = db;

        public async Task<RegistrationResult> RegisterUserAsync(UserManager<ApplicationUser> userManager, PersonRegistration dto, CancellationToken ct)
        {
            // Transaction launched on the DbContext level - to ensure both Person and ApplicationUser creations are atomic
            using var transaction = await this.db.Database.BeginTransactionAsync(ct);
            try
            {
                var emailTaken = await IsEmailTakenAsync(userManager, dto.Email.ToLower(), ct); 
                if (emailTaken)
                    RegistrationResult.EmailAlreadyUsed();

                // step 1 - creating person (table 'osoba')
                var person = CreatePersonFromDto(dto);
                await this.db.Persons.AddAsync(person, ct);
                // we need to save changes in order to obtain personId (ID is generated in DB), 
                await this.db.SaveChangesAsync(ct); // not committed yet
                // step 2 - creating Identity User
                var newUser = CreateAppUser(dto, person.Id);

                // UserManager in EF Core automatically detects existing DB transaction of the DbContext
                var identityResult = await userManager.CreateAsync(newUser, dto.Password);

                if (!identityResult.Succeeded) // if Identity fails (e.g. weak password)
                {
                    await transaction.RollbackAsync(ct);
                    
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    return RegistrationResult.Failure($"Failed to create user: {errors}");
                }
                // step 3 - assignment of a role (optional)
                 await userManager.AddToRoleAsync(newUser, "User");

                // step 4 - OK, commit transaction
                await transaction.CommitAsync(ct);

                return RegistrationResult.Success;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct); // in case of unexpected error, rollback transaction
                // optionally log the exception here
                return RegistrationResult.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task RefreshUserAsync(CancellationToken ct = default)
        {
            // Týmto donútime Blazor, aby znova zavolal GetAuthenticationStateAsync,
            // ktorý znova zavolá /api/auth/user-info a stiahne aktuálne roly.
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task<bool> IsEmailTakenAsync(UserManager<ApplicationUser> userManager, string loweredEmail, CancellationToken ct = default)
        {
            var existingUser = await userManager.FindByEmailAsync(loweredEmail);
            if (existingUser != null)
                return true;
            // using ToLower() for better translation into SQL in EF Core
            var existingPerson = await this.db.Persons
                .FirstOrDefaultAsync(p => p.Email.ToLower() == loweredEmail, ct);
            return (existingPerson != null);
        }

        private static Models.Entities.Person CreatePersonFromDto(PersonRegistration dto)
        {
            return new Models.Entities.Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                BirthDate = dto.BirthDate,
                RegisteredAt = DateTime.Now,
                Email = dto.Email, // Email ukladáme tak ako prišiel, pre zobrazenie
                Gender = dto.Gender,
                Phone = dto.Phone
            };
        }

        private static ApplicationUser CreateAppUser(PersonRegistration dto, int personId)
        {
            return new ApplicationUser
            {
                UserName = dto.Email, // Identity will normalize it automatically into NormalizedUserName
                Email = dto.Email,    // Identity will normalize it automatically into NormalizedEmail
                PersonId = personId,  // obtained person ID
                EmailConfirmed = true // if needed, email confirmation can be implemented later
            };
        }
    }
}
