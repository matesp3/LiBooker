using Microsoft.EntityFrameworkCore;
using LiBookerWebApi.Model;
using LiBooker.Shared.DTOs;

namespace LiBookerWebApi.Services
{
    public class PersonService : IPersonService
    {
        private readonly LiBookerDbContext _db;

        public PersonService(LiBookerDbContext db) => this._db = db;

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
    }
}