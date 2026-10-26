using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PeterPedal.Api.Data;

namespace PeterPedal.Api.Controllers;

// For the supplier's system, not for employees. It sends an API key instead of a JWT,
// and the key can only do one thing: update spare part prices (least privilege between systems).
[ApiController]
[Route("api/supplier")]
public class SupplierController : ControllerBase
{
    private readonly PeterPedalDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(PeterPedalDbContext db, IConfiguration config, ILogger<SupplierController> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    [HttpPut("spareparts/{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, PriceUpdate update)
    {
        // TODO: Compare the X-Api-Key header with Supplier:ApiKey using CryptographicOperations.FixedTimeEquals: 401 if it is missing or wrong (and log it). Then set the part's price: 204, or 404 if the part does not exist.
        throw new NotImplementedException();
    }
}

// [Range] makes [ApiController] answer 400 Bad Request for a negative price.
public record PriceUpdate([Range(0, 1_000_000)] decimal Price);
