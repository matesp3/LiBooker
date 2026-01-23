
using LiBooker.Shared.DTOs;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Services;

namespace LiBookerWebApi.Endpoints
{
    public static class LoanEndpoints
    {
        public static void MapLoanEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/loans");

            MapGetLoansForPersonAsync(group);   // GET  /api/loans/loan/{personId}
            MapPutLoanDatesAsync(group);        // PUT  /api/loans/loan/edit/dates
            MapPostNewLoanRequestAsync(group);  // POST /api/loans/loan/new
        }

        private static void MapPostNewLoanRequestAsync(RouteGroupBuilder group)
        {
            group.MapPost("/loan/new", async (
                LoanRequest dto,
                ILoanService svc,
                CancellationToken ct
            ) =>
            {
                try
                {
                    if (ParamChecker.IsInvalidId(dto.PersonId))
                        return Results.BadRequest("Invalid person ID.");
                    if (ParamChecker.IsInvalidId(dto.PublicationId))
                        return Results.BadRequest("Invalid publication ID.");
                    var createdLoan = await svc.AddNewLoanRequestAsync(dto, ct);
                    return createdLoan is not null
                            ? Results.Ok(createdLoan)
                            : Results.Conflict("Could not create the loan request - publication is unavailable");
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.InternalServerError();
                }
            })
            .RequireAuthorization(AuthPolicies.RequireLibrarian)
            .WithName("AddNewLoan");
        }

        private static void MapPutLoanDatesAsync(RouteGroupBuilder group)
        {
            group.MapPut("/loan/edit/dates", async (
                LoanInfo dto,
                ILoanService svc,
                CancellationToken ct
            ) =>
            {
                try
                {
                    var updatedDto = await svc.EditLoanDatesAsync(dto, ct);
                    return updatedDto is not null
                        ? Results.Ok(updatedDto)
                        : Results.NotFound();
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.InternalServerError();
                }
            })
            .RequireAuthorization(AuthPolicies.RequireLibrarian)
            .WithName("EditLoan");
        }

        private static void MapGetLoansForPersonAsync(RouteGroupBuilder group)
        {
            group.MapGet("/loan/{personId:int}", async (
                int personId,
                ILoanService svc,
                CancellationToken ct
            ) =>
            {
                try
                {
                    if (ParamChecker.IsInvalidId(personId))
                        return Results.BadRequest("Invalid person ID.");
                    var loans = await svc.GetLoansByPersonIdAsync(personId, ct);
                    return loans is not null
                        ? Results.Ok(loans)
                        : Results.NotFound();
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.InternalServerError();
                }
            })
            .RequireAuthorization(AuthPolicies.RequireLoggedUser)
            .WithName("GetLoansByPersonId");
        }
    }
}
