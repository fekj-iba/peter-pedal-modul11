using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.GraphQL;

// Every public method becomes a field on the GraphQL Mutation type.
// If the case or part does not exist, the method returns null (or false for DeleteCase).
public class Mutation
{
    // TODO: Only the scope cases:create may create cases.
    public async Task<RepairCase> CreateCase(int customerId, string frameNumber, string problem,
        [Service] PeterPedalDbContext db)
    {
        var repairCase = new RepairCase { CustomerId = customerId, FrameNumber = frameNumber, Problem = problem };
        db.RepairCases.Add(repairCase);
        await db.SaveChangesAsync();
        await db.Entry(repairCase).Reference(c => c.Customer).LoadAsync();
        return repairCase;
    }

    // TODO: Only the scope cases:repair may add parts.
    public async Task<RepairCase?> AddPart(int caseId, int partId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        var part = await db.SpareParts.FindAsync(partId);
        if (repairCase is null || part is null)
        {
            return null;
        }
        if (!repairCase.Parts.Contains(part))
        {
            repairCase.Parts.Add(part);
            await db.SaveChangesAsync();
        }
        return repairCase;
    }

    // TODO: Only the scope cases:repair may calculate the offer.
    public async Task<RepairCase?> CalculateOffer(int caseId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        if (repairCase is null)
        {
            return null;
        }
        repairCase.Price = PriceCalculator.CalculateOffer(repairCase.Parts);
        await db.SaveChangesAsync();
        return repairCase;
    }

    // TODO: Only the scope cases:status may set the status.
    public async Task<RepairCase?> SetStatus(int caseId, CaseStatus status, [Service] PeterPedalDbContext db)
    {
        var repairCase = await Load(db, caseId);
        if (repairCase is null)
        {
            return null;
        }
        repairCase.Status = status;
        await db.SaveChangesAsync();
        return repairCase;
    }

    // TODO: Only the scope admin:delete may delete cases.
    public async Task<bool> DeleteCase(int caseId, [Service] PeterPedalDbContext db)
    {
        var repairCase = await db.RepairCases.FindAsync(caseId);
        if (repairCase is null)
        {
            return false;
        }
        db.RepairCases.Remove(repairCase);
        await db.SaveChangesAsync();
        return true;
    }

    private static Task<RepairCase?> Load(PeterPedalDbContext db, int caseId) =>
        db.RepairCases.Include(c => c.Customer).Include(c => c.Parts)
            .FirstOrDefaultAsync(c => c.Id == caseId);
}
