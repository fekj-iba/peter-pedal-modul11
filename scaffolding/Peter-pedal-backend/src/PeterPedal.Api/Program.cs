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
// TODO: Task 2: check the JWT from task 1 (authentication), and add one authorization policy per scope.
// - builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => ...)
// - options.TokenValidationParameters = new TokenValidationParameters { ... }: the same issuer, audience
//   and key as in task 1. An expired token must be rejected at once (look up ClockSkew).
// - Keep the claim names from the token (look up MapInboundClaims), and say that the user's name is in
//   the claim "name" (NameClaimType).
// - builder.Services.AddAuthorization(options => ...): one policy per scope in Scopes.All
//   (options.AddPolicy) that requires the claim "scope" with that value (RequireClaim).

// --- Rate limiting: slows down password guessing ---
// TODO: Task 5: limit login attempts per IP address.
// - builder.Services.AddRateLimiter(options => ...), and give your rule a name with options.AddPolicy.
// - One counter per IP address (context.Connection.RemoteIpAddress): RateLimitPartition.GetFixedWindowLimiter.
// - RateLimiting:LoginPermitsPerMinute attempts per minute: FixedWindowRateLimiterOptions (PermitLimit, Window).
// - The status code for "too many requests": options.RejectionStatusCode. The default is 503.

// --- Logging of security events: what happened, to whom, from where. Never passwords or tokens. ---
// TODO: Task 6: log every 401 and 403.
// - JWT bearer calls events when it rejects a call: options.Events = new JwtBearerEvents { ... }.
//   Set them here with builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
//   options => ...), or in AddJwtBearer from task 2.
// - OnChallenge is called for 401, OnForbidden for 403. Each gets a context and returns Task.CompletedTask.
// - Log method, path and (for 403) the user's name with LogWarning. Get a logger from
//   context.HttpContext.RequestServices (ILoggerFactory). Never log the token.

// --- GraphQL: Query and Mutation classes in GraphQL/, served at /graphql ---
builder.Services
    .AddGraphQLServer()
    // TODO: Task 2: GraphQL must use the authorization policies too.
    // Hot Chocolate needs one extra method call here on the GraphQL server.
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
// TODO: Task 2: turn on authentication and authorization.
// The order matters: "who are you?" must come before "may you do this?". Both after UseCors.
// TODO: Task 5: turn on rate limiting (after authorization).
app.MapControllers();
app.MapGraphQL();

app.Run();

// Makes Program visible to WebApplicationFactory in the test project.
public partial class Program { }
