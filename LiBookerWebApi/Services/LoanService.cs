using LiBooker.Shared.DTOs;
using LiBookerWebApi.Model;
using LiBookerWebApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace LiBookerWebApi.Services
{
    public class LoanService(LiBookerDbContext db) : ILoanService
    {
        private static readonly int MaxDaysLoaned = 30;
        private readonly LiBookerDbContext _db = db;

        public async Task<LoanInfo?> AddNewLoanRequestAsync(LoanRequest dto, CancellationToken ct)
        {
            using var transaction = await _db.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var availableCopyId = await _db.Copies
                    .Where(c => c.PublicationId == dto.PublicationId &&
                               !_db.Loans.Any(l => l.CopyId == c.Id && l.ReturnedAt == null))
                    .Select(copy => new { copy.Id })
                    .FirstOrDefaultAsync(ct).ConfigureAwait(false);

                if (availableCopyId is null)
                    return null;

                //var nextLoanId = await GetNextLoanIdAsync(ct).ConfigureAwait(false);
                int? nextLoanId = await GetNextSequenceValueAsync("VYPOZICANIE_SEQ");
                //Console.WriteLine($"RETRIEVED LOAN ID = {nextLoanId}");
                var newLoan = new LoanEf
                {
                    Id = nextLoanId ?? -1,
                    PersonId = dto.PersonId,
                    CopyId = availableCopyId.Id,
                    LoanedAt = DateTime.Now,
                    DueAt = DateTime.Now.AddDays(MaxDaysLoaned),
                    FineId = null
                };

                await _db.LoansEf.AddAsync(newLoan, ct).ConfigureAwait(false);
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);

                var bookTitle = await _db.Publications
                        .Where(p => p.Id == dto.PublicationId)
                        .Select(p => p.Book.Title)
                        .FirstOrDefaultAsync(ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);
                return new LoanInfo
                {
                    LoanId = newLoan.Id,
                    PublicationId = dto.PublicationId,
                    BookTitle = bookTitle ?? "Unknown",
                    DateFrom = newLoan.LoanedAt,
                    DateTo = newLoan.DueAt,
                    ReturnDate = newLoan.ReturnedAt
                };
            }
            catch (Exception ex)
            {
                _ = ex;
                Console.WriteLine(ex.Message);
                await transaction.RollbackAsync(ct);
                return null;
            }
        }

        public async Task<LoanInfo?> EditLoanDatesAsync(LoanInfo dto, CancellationToken ct)
        {
            var loan = await _db.Loans
                .Where(l => l.Id == dto.LoanId)
                .FirstOrDefaultAsync(ct).ConfigureAwait(false);
            if (loan == null)
                return null;

            loan.LoanedAt = dto.DateFrom;
            loan.DueAt = dto.DateTo;
            loan.ReturnedAt = dto.ReturnDate;
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return dto;
        }

        public async Task<List<LoanInfo>?> GetLoansByPersonIdAsync(int personId, CancellationToken ct)
        {
            var results = await _db.Loans
                .Where(l => l.PersonId == personId)
                .Select(l => new LoanInfo
                {
                    LoanId = l.Id,
                    PublicationId = l.Copy!.PublicationId,
                    BookTitle = l.Copy!.Publication!.Book.Title,
                    DateFrom = l.LoanedAt,
                    DateTo = l.DueAt,
                    ReturnDate = l.ReturnedAt
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return results;
        }

        private async Task<int> GetNextLoanIdAsync(CancellationToken ct)
        {
            return await _db.Database
                .SqlQueryRaw<int>("SELECT VYPOZICANIE_SEQ.NEXTVAL FROM DUAL")
                .SingleAsync(ct);
        }

        public async Task<int?> GetNextSequenceValueAsync(string sequenceName)
        {
            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                if (command == null || command.Connection == null)
                    return null;
                // SQL musí byť čo najjednoduchšie
                command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
                command.CommandType = CommandType.Text;

                if (command.Connection.State != ConnectionState.Open)
                    await command!.Connection!.OpenAsync();

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }
    }
}
