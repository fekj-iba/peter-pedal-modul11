using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeterPedal.Api.Auth;
using PeterPedal.Api.Data;
using PeterPedal.Api.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// The browser only lets the frontend call this API if the API allows its origin (CORS).
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(builder.Configuration["FrontendOrigin"]!).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<PeterPedalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PeterPedal")));

// --- REST: controllers in Controllers/, documented by Swagger at /swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// The "Authorize" button in Swagger UI: paste a token from POST /api/auth/login, and Swagger sends it
// as "Authorization: Bearer <token>" on every request.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Indsæt tokenet fra POST /api/auth/login (uden \"Bearer \")."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- Security: who are you (authentication) and what may you do (authorization) ---
// TODO: Validate the JWT with AddJwtBearer (issuer, audience, signing key from Jwt:Key, lifetime with ClockSkew = TimeSpan.Zero, MapInboundClaims = false, NameClaimType = "name"), and add one authorization policy per scope in Scopes.All that requires the claim "scope".

// --- Rate limiting: slows down password guessing ---
// TODO: Add a fixed-window rate limiter policy named "login": RateLimiting:LoginPermitsPerMinute requests per minute per IP address, answering 429.

// --- Logging of security events: what happened, to whom, from where. Never passwords or tokens. ---
// TODO: Log every 401 and 403 with JwtBearerEvents (OnChallenge and OnForbidden): method, path and (for 403) the user's name.

// --- GraphQL: Query and Mutation classes in GraphQL/, served at /graphql ---
builder.Services
    .AddGraphQLServer()
    // TODO: Let Hot Chocolate use the authorization policies (needed for [Authorize] in Query and Mutation).
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

// No migrations: create the database from the models on first start and add demo data.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PeterPedalDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);
// TODO: Turn on authentication and then authorization (after CORS).
// TODO: Turn on rate limiting.
app.MapControllers();
app.MapGraphQL();

app.Run();

// Makes Program visible to WebApplicationFactory in the test project.
public partial class Program { }
