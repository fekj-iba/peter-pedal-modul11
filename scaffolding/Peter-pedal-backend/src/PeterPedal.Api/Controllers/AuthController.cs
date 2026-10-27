using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;

namespace PeterPedal.Api.Controllers;

// Authentication: who are you? Checks the password and hands out an access token (a JWT).
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // TODO: Task 1: which scopes (see Auth/Scopes.cs) does each role need – and no more?
    // The keys are the role names: "FrontDesk", "Mechanic", "Owner" and "Customer".
    // The values are arrays of constants from Scopes, e.g. new[] { Scopes.WorkshopRead, ... }.
    // Use the scope table in the README: what does the front desk do, and what does a mechanic do?
    // The owner may do everything (Scopes.All). A customer has no scopes (Array.Empty<string>()).
    private static readonly Dictionary<string, string[]> ScopesByRole = new();

    private readonly PeterPedalDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(PeterPedalDbContext db, IConfiguration config, ILogger<AuthController> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    [HttpPost("login")]
    // TODO: Task 5: use your rate limiting rule from Program.cs here.
    // An attribute with the name you gave the rule: [EnableRateLimiting("...")].
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // TODO: Task 1: check the username and password, and answer with a JWT.
        // 1. Find the user with that username in _db.Users (SingleOrDefaultAsync).
        // 2. Check the password with PasswordHasher.Verify. Unknown user or wrong password: the same error
        //    response, with the right status code (Problem(title: ..., statusCode: ...)).
        // 3. Make a List<Claim>: the user's id (which standard claim?), "name", one "scope" claim per scope in
        //    ScopesByRole[user.Role], and "customer_id" if user.CustomerId has a value.
        // 4. Make the token: new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor { ... }).
        //    Subject (the claims), Issuer, Audience, IssuedAt, NotBefore, Expires and SigningCredentials.
        //    The values are in the Jwt section of appsettings.json (_config["Jwt:..."]). The key must be
        //    a SymmetricSecurityKey made from the bytes of Jwt:Key; sign with HMAC SHA-256.
        // 5. Return new LoginResponse(token).
        // Task 6: log failed logins with _logger.LogWarning: who and from which IP address. Never the password.
        throw new NotImplementedException();
    }
}

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token);
