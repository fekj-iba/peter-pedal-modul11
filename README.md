# Peter Pedal – Modul 11: API-sikkerhed

Vi forsætter arbejdet med API i Peter Pedals Cykelbutik. I dag har vi fokus på API sikkerhed. i skal arbejde videre i **jeres eget repository fra modul 9**. Dette
repository indeholder kun opgaverne og de filer, som skal kopieres ind:

- `scaffolding/` – nye og ændrede filer (brugere, login-side, kundeside, scripts m.m.). Alle skal bruge dem.
- `catch-up/` – løsningen på modul 9 (`SparePartsController.cs`, `Mutation.cs`, `reservedele.js`). Kun hvis du
  ikke blev færdig med modul 9.

## Trin 0 – Kom i gang (5 min)

1. Blev du ikke færdig med modul 9? Kopiér indholdet af `catch-up/` ind over roden af dit repository.
2. Kopiér indholdet af `scaffolding/` ind over roden af dit repository. Den overskriver ingen af de filer, du
   skrev i modul 9.
   - Linux: `cp -R scaffolding/. ../dit-repository/`
   - Windows: åbn `scaffolding` i Stifinder, markér alt, og kopiér det ind i roden af dit repository.
3. Stop API'et, og slet `Peter-pedal-backend/src/PeterPedal.Api/peterpedal.db`. Databasen bliver kun oprettet,
   når filen ikke findes, og der er nye tabeller og data.
4. Start API'et og frontenden som i modul 9. I Swagger (http://localhost:5080/swagger) kan du nu se
   `POST /api/auth/login` og en **Authorize**-knap.

### Brugere

Medarbejderne logger ind – og nu kan kunderne også logge ind og følge deres egne sager.

| Brugernavn | Adgangskode | Rolle |
|---|---|---|
| `frida` | `frida123` | FrontDesk (skranken) |
| `bo` | `bo123` | Mechanic |
| `anna` | `anna123` | Mechanic |
| `peter` | `peter123` | Owner |
| `egon` | `egon123` | Customer (Egon, kunde 1) |
| `lise` | `lise123` | Customer (Lise, kunde 2) |

Egon har sag 1, og Lise har sag 2. Alle mekanikere må arbejde på alle sager.

### Scopes

Et token indeholder de scopes, brugeren har. Hvert scope giver lov til bestemte ting:

| Scope | Giver lov til |
|---|---|
| `workshop:read` | Læse kunder, reservedele og alle sager |
| `customers:write` | Oprette og rette kunder |
| `parts:write` | Oprette og rette reservedele |
| `cases:create` | `createCase` |
| `cases:status` | `setStatus` |
| `cases:repair` | `addPart`, `calculateOffer` |
| `admin:delete` | Alle sletninger |

En kunde har ingen scopes. Kundens token siger i stedet, hvilken kunde det er (claim'en `customer_id`), og en
kunde må kun læse sine egne sager. Konstanterne ligger i `Auth/Scopes.cs`.

## Opgave 1 – Log ind (30 min)

`Controllers/AuthController.cs`, `POST /api/auth/login`:

1. Fyld `ScopesByRole` ud: hvilke scopes skal FrontDesk, Mechanic, Owner og Customer have? Giv hver rolle
   **kun** det, den har brug for. Ejeren skal kunne alt.
2. Find brugeren i `_db.Users`, og tjek adgangskoden med `PasswordHasher.Verify`. Forkert brugernavn og
   forkert adgangskode skal give **samme** svar: `401` med titlen "Forkert brugernavn eller adgangskode."
   (`Problem(title: ..., statusCode: StatusCodes.Status401Unauthorized)`).
3. Lav claims: `sub` (brugerens id), `name` og én `scope`-claim pr. scope. Er brugeren en kunde
   (`CustomerId` er ikke `null`), så tilføj også `customer_id`.
4. Lav tokenet med `new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor { ... })`:
   `Subject`, `Issuer` og `Audience` (fra `Jwt:Issuer`/`Jwt:Audience`), `IssuedAt`, `NotBefore`, `Expires`
   (`Jwt:LifetimeMinutes` minutter frem) og `SigningCredentials` med `Jwt:Key` og `SecurityAlgorithms.HmacSha256`.
5. Returnér `new LoginResponse(token)`.

**Test:** Log ind som `frida` i Swagger. Kopiér tokenet ind på https://jwt.io, og find `sub`, `name`, `scope`
og `exp`. Gør det samme for `lise` – hvad er anderledes? Prøv også en forkert adgangskode og et ukendt
brugernavn.

## Opgave 2 – Lås API'et (30 min)

**Tjek tokenet** i `Program.cs` (se `// TODO`):

- `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` med `TokenValidationParameters`:
  `ValidIssuer`, `ValidAudience`, `IssuerSigningKey` (samme nøgle som i opgave 1), `ClockSkew = TimeSpan.Zero`
  og `NameClaimType = "name"`. Sæt også `options.MapInboundClaims = false`, ellers omdøber ASP.NET `sub` og
  `scope` til lange navne.
- `AddAuthorization`: én policy pr. scope i `Scopes.All`: `policy.RequireClaim("scope", scope)`.
- `.AddAuthorization()` på GraphQL-serveren.
- `app.UseAuthentication()` og `app.UseAuthorization()` efter `UseCors`.

**Brug reglerne:**

- REST: `[Authorize(Policy = Scopes.WorkshopRead)]` på `CustomersController` og `SparePartsController` (fra
  `Microsoft.AspNetCore.Authorization`), så kun medarbejdere kommer ind. `[Authorize(Policy = Scopes.CustomersWrite)]`
  osv. på `Create`/`Update`, `Scopes.AdminDelete` på `Delete`.
- GraphQL: `[Authorize]` på `Query` (alle, der er logget ind), `Scopes.WorkshopRead` på `RepairCases` (listen over
  alle sager) og en policy på hver metode i `Mutation`. **Brug `HotChocolate.Authorization`**, ikke ASP.NET's
  `[Authorize]` – de hedder det samme.
- Bruger du din egen `SparePartsController.cs` eller `Mutation.cs` fra modul 9, skal du selv tilføje usings.
  Lad ikke IDE'en vælge for dig – i `Mutation.cs` virker ASP.NET's `[Authorize]` ikke, men du får ingen fejl:
  - `SparePartsController.cs`: `using Microsoft.AspNetCore.Authorization;` og `using PeterPedal.Api.Auth;`
  - `Mutation.cs`: `using HotChocolate.Authorization;` og `using PeterPedal.Api.Auth;`

**Test i Swagger:** Kald `GET /api/customers` uden token (401). Log ind som `frida`, tryk **Authorize**, og
indsæt tokenet. Opret en kunde (201), og opret en reservedel (403). I GraphQL (http://localhost:5080/graphql)
sætter du headeren `Authorization: Bearer <token>`. Hvad står der i `errors`, når du ikke må? Log til sidst ind
som `lise`: må hun se kunderne eller listen over alle sager?

## Opgave 3 – Frontenden (20 min)

1. `js/auth.js`: gem, hent og slet tokenet i `sessionStorage`.
2. `js/login.js`: send brugernavn og adgangskode til `/api/auth/login` med `fetch`. Vis en fejl ved 401 og
   429. Ellers: gem tokenet. En kunde (tokenet har claim'en `customer_id`) går til `min-sag.html`, en
   medarbejder til `sager.html`. `readClaims` i `auth.js` læser claims'ene i tokenet.
3. `js/rest.js` og `js/graphql.js`: send headeren `Authorization: Bearer <token>`. Gå til login-siden ved
   `401` (REST) eller fejlkoden `AUTH_NOT_AUTHENTICATED` (GraphQL).

**Test:** Log ind som `frida` på http://localhost:5100/login.html. Opret en kunde og en reservedel. Kig i
Network-fanen efter `Authorization`-headeren. Vent 2 minutter, og klik videre. Hvad sker der? Log ind som
`lise`: du lander på `min-sag.html`. Skriv sagsnummer 2.

## Opgave 4 – Autorisation på objektniveau (25 min)

Lise må gerne spørge efter en sag med `repairCase(id: ...)` – men kun sin egen. Scopes kan ikke klare det:
reglen afhænger af, *hvilken* sag hun spørger efter.

1. Kør angrebet (API'et skal køre):

       dotnet run --project scripts/PeterPedal.Scripts -- attack

   Lise logger ind og spørger efter sag 1, som er Egons. Hvad kan hun se? Prøv også i browseren: log ind som
   `lise`, og ret adressen fra `min-sag.html?id=2` til `min-sag.html?id=1`.
2. Lav tjekket i `GraphQL/Query.cs`, i `RepairCase` (se `// TODO`):
   - Har den kaldende scope'et `workshop:read`, er det en medarbejder, som må se alle sager:
     `user.HasClaim("scope", Scopes.WorkshopRead)`.
   - Ellers er det en kunde. Kundens id står i claim'en `customer_id`: `user.FindFirst("customer_id")?.Value`.
     Hører sagen til en anden kunde, så returnér `null` – præcis som hvis sagen ikke fandtes. (Logningen af
     forsøget, som `// TODO` også nævner, kan vente til opgave 6.)
3. Kør angrebet igen, og prøv `?id=1` i browseren igen. Hvorfor `null` og ikke en fejlbesked som "Det er ikke
   din sag"?

## Opgave 5 – Rate limiting (10 min)

1. `Program.cs`: `AddRateLimiter` med en fixed window-policy `"login"` pr. IP-adresse
   (`RateLimitPartition.GetFixedWindowLimiter`) med `RateLimiting:LoginPermitsPerMinute` forsøg pr. minut.
   `RejectionStatusCode = 429`. Husk `app.UseRateLimiter()`.
2. `AuthController`: `[EnableRateLimiting("login")]` på `Login`.
3. Test med en angriber, der gætter Peters adgangskode (den rigtige er nr. 8):

       dotnet run --project scripts/PeterPedal.Scripts -- guess

   Har du selv logget ind inden for det sidste minut, tæller de forsøg også med.

## Opgave 6 – Logning (10 min)

Log med `ILogger` (`LogWarning`) – men **aldrig** adgangskoder eller tokens:

1. Mislykket login i `AuthController`: brugernavn og IP-adresse (`HttpContext.Connection.RemoteIpAddress`).
2. 401 og 403 i `Program.cs`: `JwtBearerEvents` med `OnChallenge` og `OnForbidden` (metode, sti og – ved 403 –
   brugerens navn).
3. Afvist sag i `Query.RepairCase`: hvem prøvede at læse hvilken sag.

**Test:** Gentag testene fra opgave 2 og 4, og kig i API'ets konsol. Bemærk: `JwtBearerEvents` logger kun
REST-kaldene. Når GraphQL afviser et kald (`AUTH_NOT_AUTHORIZED`), kommer der ingen 403-linje – hvorfor ikke?

## Opgave 7 – Hvis du når det: API-nøgle mellem systemer

Leverandørens system skal kunne opdatere priser på reservedele – og intet andet. Det logger ikke ind; det
sender en API-nøgle i headeren `X-Api-Key`.

1. `Controllers/SupplierController.cs`, `PUT /api/supplier/spareparts/{id}/price`: sammenlign headeren med
   `Supplier:ApiKey` med `CryptographicOperations.FixedTimeEquals`. Forkert eller manglende nøgle: `401`.
   Ellers: sæt prisen (`204`), eller `404` hvis reservedelen ikke findes.
2. Test (nøglen står i `appsettings.Development.json`):

       dotnet run --project scripts/PeterPedal.Scripts -- supplier
       dotnet run --project scripts/PeterPedal.Scripts -- supplier supplier-dev-key-7f3a9c

3. Prøv nøglen på `PUT /api/spareparts/1` i stedet. Hvad sker der?
