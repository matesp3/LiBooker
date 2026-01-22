using LiBooker.Shared.ApiResponses;
using LiBooker.Shared.DTOs;
using LiBooker.Shared.Roles;
using LiBookerWebApi.Endpoints.ResultWrappers;
using LiBookerWebApi.Model;
using LiBookerWebApi.Models;
using LiBookerWebApi.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static LiBookerWebApi.Utils.PersonUploader;

namespace LiBookerWebApi.Services
{
    public class AuthService(LiBookerDbContext db, UserManager<ApplicationUser> userManager) : IAuthService
    {
        private readonly LiBookerDbContext _db = db;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<RegistrationResult> RegisterUserAsync(PersonRegistration dto, CancellationToken ct)
        {
            // Transaction launched on the DbContext level - to ensure both Person and ApplicationUser creations are atomic
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var emailTaken = await IsEmailTakenAsync(dto.Email.ToLower(), ct);
                if (emailTaken)
                    RegistrationResult.EmailAlreadyUsed();

                // step 1 - creating person (table 'osoba')
                var person = CreatePersonFromDto(dto);
                await _db.Persons.AddAsync(person, ct);
                // we need to save changes in order to obtain personId (ID is generated in DB), 
                await _db.SaveChangesAsync(ct); // not committed yet
                // step 2 - creating Identity User
                var newUser = CreateAppUser(dto, person.Id);

                // UserManager in EF Core automatically detects existing DB transaction of the DbContext
                var identityResult = await _userManager.CreateAsync(newUser, dto.Password);
                
                if (!identityResult.Succeeded) // if Identity fails (e.g. weak password)
                {
                    await transaction.RollbackAsync(ct);
                    
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    return RegistrationResult.Failure($"Failed to create user: {errors}");
                }
                // step 3 - assignment of a default role
                await _userManager.AddToRoleAsync(newUser, UserRolesExtensions.GetRoleName(UserRoles.User));

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

        public async Task<UserInfoResponse?> GetUserInfoAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return null;

            var roles = await _userManager.GetRolesAsync(appUser);

            return new UserInfoResponse()
            {
                PersonId = appUser?.PersonId,
                Email = appUser?.Email ?? "Unknown",
                Roles = [.. roles]
            };
        }

        private async Task<bool> IsEmailTakenAsync(string loweredEmail, CancellationToken ct = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(loweredEmail);
            if (existingUser != null)
                return true;
            // using ToLower() for better translation into SQL in EF Core
            var existingPerson = await _db.Persons
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


        public async Task<List<UserAccountDto>> CreateUserForPerson(List<UserAccountDto> users, ILogger<Program> logger, CancellationToken token)
        {
            int i = 0;
            var roleName = UserRolesExtensions.GetRoleName(UserRoles.User);
            var transaction = await _db.Database.BeginTransactionAsync(token).ConfigureAwait(false);
            try
            {
                foreach (var userDto in users)
                {
                    var person = _db.Persons.FirstOrDefault(p => p.Id == userDto.PersonId);
                    if (person == null)
                    {
                        logger.LogError("Person with ID {PersonId} not found for email {Email}", userDto.PersonId, userDto.Email);
                        continue;
                    }
                    var existingUser = await _userManager.FindByEmailAsync(userDto.Email).ConfigureAwait(false);
                    if (existingUser != null)
                    { // ok.. user is paired with personId
                        logger.LogWarning("User for email {Email} already exists, skipping creation", userDto.Email);
                        continue;
                    }
                    var newUser = new ApplicationUser
                    {
                        UserName = userDto.Email,
                        Email = userDto.Email,
                        PersonId = userDto.PersonId,
                        EmailConfirmed = true // if needed, email confirmation can be implemented later
                    };
                    var password = PasswordGenerator.Generate(8); // 8-chars password
                    userDto.Password = password;
                    var result = await _userManager.CreateAsync(newUser, password);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create user for email {Email}: {Errors}", userDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue;
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, roleName).ConfigureAwait(false);
                        logger.LogInformation("{i}. Created user for email {Email} with role {roleName}", ++i, userDto.Email, roleName);
                    }

                }
                transaction.Commit();
                return users;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token).ConfigureAwait(false);
                logger.LogError("Exception during user creation: {Message}", ex.Message);
                return users;
            }
        }
    }
}
