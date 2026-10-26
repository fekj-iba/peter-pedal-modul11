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
    // TODO: Least privilege: give each role (FrontDesk, Mechanic, Owner, Customer) only the scopes from Scopes that it needs. A customer needs none: they may only read their own cases.
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
    // TODO: Rate limiting: use the "login" policy from Program.cs on this action.
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // TODO: Find the user and check the password with PasswordHasher.Verify (401 with the same message for an unknown user and a wrong password). Then return a JWT signed with Jwt:Key that holds sub, name, one scope claim per scope and (for a customer) customer_id, and expires after Jwt:LifetimeMinutes.
        throw new NotImplementedException();
    }
}

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token);
