using Microsoft.EntityFrameworkCore;
using LiBookerWebApi.Model;
using LiBooker.Shared.DTOs;
using LiBookerShared.ApiResponses;
using Microsoft.AspNetCore.Identity;
using LiBookerWebApi.Models;

namespace LiBookerWebApi.Services
{
    public class PersonService : IPersonService
    {
        private static readonly string[] s_validGenderCodes = {"M", "F", "N"};
        private readonly LiBookerDbContext _db;

        public PersonService(LiBookerDbContext db) => _db = db;

        public async Task<List<Person>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Persons
                .AsNoTracking()
                .Select(p => new Person
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    BirthDate = p.BirthDate,
                    RegisteredAt = p.RegisteredAt,
                    ReservationCount = _db.Reservations.Count(r => r.PersonId == p.Id),
                    LoanCount = _db.Loans.Count(l => l.PersonId == p.Id)
                })
                .ToListAsync(ct);
        }

        public async Task<Person?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Persons
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new Person
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    BirthDate = p.BirthDate,
                    RegisteredAt = p.RegisteredAt
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<UpdateResponse<PersonUpdate>> UpdateAsync(
            int id, 
            PersonUpdate dto, 
            UserManager<ApplicationUser> userManager, 
            CancellationToken ct)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {   // person user update
                var personInDb = await _db.Persons.FirstOrDefaultAsync(p => p.Id == id, ct);
                if (personInDb == null)
                    return UpdateResponse<PersonUpdate>.Failure("Person not found.");

                personInDb.FirstName = dto.FirstName;
                personInDb.LastName = dto.LastName;
                var oldEmail = personInDb.Email;
                personInDb.Email = dto.Email;
                personInDb.Phone = dto.Phone;
                personInDb.BirthDate = dto.BirthDate;
                dto.Gender = char.ToUpper(dto.Gender);
                if (s_validGenderCodes.Any(s => s.Equals(dto.Gender.ToString())))
                    personInDb.Gender = dto.Gender;
                await _db.SaveChangesAsync(ct); // save person changes without commit

                var err = await UpdateIdentityEmail(id, dto, userManager, oldEmail, ct).ConfigureAwait(false);
                if (err is null)
                { 
                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                    return UpdateResponse<PersonUpdate>.Success(dto);
                }
                await transaction.RollbackAsync(ct).ConfigureAwait(false);
                return UpdateResponse<PersonUpdate>.Failure(err);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                // Log exception...
                return UpdateResponse<PersonUpdate>.Failure($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <param name="userManager"></param>
        /// <param name="transaction"></param>
        /// <param name="oldEmail"></param>
        /// <param name="ct"></param>
        /// <returns>error in case of exception. If successful, then null is returned</returns>
        private static async Task<string?> UpdateIdentityEmail(int id, PersonUpdate dto, 
            UserManager<ApplicationUser> userManager, string oldEmail, CancellationToken ct)
        {
            try
            {
                // IDENTITY user update
                if (!string.Equals(oldEmail, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var appUser = await userManager.Users.FirstOrDefaultAsync(u => u.PersonId == id, ct);
                    if (appUser != null)
                    {
                        var emailResult = await userManager.SetEmailAsync(appUser, dto.Email);
                        if (!emailResult.Succeeded)
                        {
                            var errors = string.Join(", ", emailResult.Errors.Select(e => e.Description));
                            return $"Failed to update email in Identity: {errors}";
                        }
                        var userResult = await userManager.SetUserNameAsync(appUser, dto.Email); // email as username
                        if (!userResult.Succeeded)
                        {
                            return "Failed to update username with email in Identity.";
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return $"Failed during IDENTITY email update: {e.Message}";
            }
        }
    }
}