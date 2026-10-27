using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.Controllers;

[ApiController]
[Route("api/spareparts")]
// TODO: Task 2: who may use this controller? Only employees, not customers.
// [Authorize(Policy = ...)] with the scope every employee has (Microsoft.AspNetCore.Authorization).
public class SparePartsController : ControllerBase
{
    private readonly PeterPedalDbContext _db;

    public SparePartsController(PeterPedalDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<SparePart>> GetAll() =>
        await _db.SpareParts.OrderBy(p => p.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SparePart>> GetById(int id)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part is null)
        {
            return NotFound();
        }
        return part;
    }

    [HttpPost]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<ActionResult<SparePart>> Create(SparePart part)
    {
        part.Id = 0; // the database assigns the id
        _db.SpareParts.Add(part);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
    }

    [HttpPut("{id}")]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<ActionResult<SparePart>> Update(int id, SparePart input)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part is null)
        {
            return NotFound();
        }
        part.Name = input.Name;
        part.Price = input.Price;
        await _db.SaveChangesAsync();
        return part;
    }

    [HttpDelete("{id}")]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _db.SpareParts.FindAsync(id);
        if (part is null)
        {
            return NotFound();
        }
        _db.SpareParts.Remove(part);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
