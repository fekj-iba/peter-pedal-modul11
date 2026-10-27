using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.GraphQL;

// Every public method becomes a field on the GraphQL Query type.
// TODO: Task 2: who may read cases? Everyone who is logged in.
// [Authorize] from HotChocolate.Authorization, not the ASP.NET one.
public class Query
{
    // TODO: Task 2: who may see the list of every case? Only employees.
    // [Authorize(Policy = ...)] with the scope every employee has (HotChocolate.Authorization).
    public async Task<List<RepairCase>> RepairCases([Service] PeterPedalDbContext db) =>
        await db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .OrderBy(c => c.Id).ToListAsync();

    // Employees may read any case; a customer only their own.
    public async Task<RepairCase?> RepairCase(int id, ClaimsPrincipal user,
        [Service] PeterPedalDbContext db, [Service] ILogger<Query> logger)
    {
        var repairCase = await db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .FirstOrDefaultAsync(c => c.Id == id);
        // TODO: Task 3 (BOLA): a customer may only read their own case.
        // - If no case was found, there is nothing to check.
        // - An employee has the scope workshop:read: user.HasClaim("scope", ...).
        // - A customer has no scopes, but their id is in the claim customer_id: user.FindFirst(...)?.Value.
        //   It is a string; the case belongs to the customer repairCase.CustomerId (an int).
        // - For another customer's case, answer exactly as if the case did not exist.
        // - Task 6: log the attempt with logger.LogWarning: who tried to read which case.
        return repairCase;
    }
}
