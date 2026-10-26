using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.GraphQL;

// Every public method becomes a field on the GraphQL Query type.
// TODO: Only logged-in users may read cases ([Authorize] from HotChocolate.Authorization, not the ASP.NET one).
public class Query
{
    // TODO: Only employees (scope workshop:read) may list every case.
    public async Task<List<RepairCase>> RepairCases([Service] PeterPedalDbContext db) =>
        await db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .OrderBy(c => c.Id).ToListAsync();

    // Employees may read any case; a customer only their own.
    public async Task<RepairCase?> RepairCase(int id, ClaimsPrincipal user,
        [Service] PeterPedalDbContext db, [Service] ILogger<Query> logger)
    {
        var repairCase = await db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .FirstOrDefaultAsync(c => c.Id == id);
        // TODO: Object level: a caller without the scope workshop:read is a customer, and may only read cases whose CustomerId matches the "customer_id" claim. For another customer's case, log who tried which case and return null, exactly as for a case that does not exist.
        return repairCase;
    }
}
