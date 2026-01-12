using Microsoft.EntityFrameworkCore;
using LiBookerWebApi.Model;
using LiBooker.Shared.DTOs;


namespace LiBookerWebApi.Services
{
    public class PersonService : IPersonService
    {
        private readonly AppDbContext db;

        public PersonService(AppDbContext db) => this.db = db;

        public async Task<List<PersonDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await db.Persons
                .AsNoTracking()
                .Select(p => new PersonDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    BirthDate = p.BirthDate,
                    RegisteredAt = p.RegisteredAt,
                    ReservationCount = db.Reservations.Count(r => r.PersonId == p.Id),
                    LoanCount = db.Loans.Count(l => l.PersonId == p.Id)
                })
                .ToListAsync(ct);
        }

        public async Task<PersonDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await db.Persons
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PersonDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    BirthDate = p.BirthDate,
                    RegisteredAt = p.RegisteredAt,
                    ReservationCount = db.Reservations.Count(r => r.PersonId == p.Id),
                    LoanCount = db.Loans.Count(l => l.PersonId == p.Id)
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}