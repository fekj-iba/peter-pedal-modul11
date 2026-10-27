using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.Controllers;

[ApiController]
[Route("api/customers")]
// TODO: Task 2: who may use this controller? Only employees, not customers.
// [Authorize(Policy = ...)] with the scope every employee has (Microsoft.AspNetCore.Authorization).
public class CustomersController : ControllerBase
{
    private readonly PeterPedalDbContext _db;

    public CustomersController(PeterPedalDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<Customer>> GetAll() =>
        await _db.Customers.OrderBy(c => c.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }
        return customer;
    }

    [HttpPost]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<ActionResult<Customer>> Create(Customer customer)
    {
        customer.Id = 0; // the database assigns the id
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<ActionResult<Customer>> Update(int id, Customer input)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }
        customer.FirstName = input.FirstName;
        customer.LastName = input.LastName;
        customer.Phone = input.Phone;
        await _db.SaveChangesAsync();
        return customer;
    }

    [HttpDelete("{id}")]
    // TODO: Task 2: which scope does this need?
    // Look in the scope table in the README, and use [Authorize(Policy = Scopes....)].
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
