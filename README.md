# Peter Pedal – Modul 11: API-sikkerhed

Vi fortsætter arbejdet med API'et i Peter Pedals Cykelbutik. I dag har vi fokus på API-sikkerhed. I skal arbejde
videre i **jeres eget repository fra modul 9**. Dette repository indeholder kun opgaverne og de filer, som skal
kopieres ind:

- `scaffolding/` – nye filer (brugere, login-side, kundeside, scripts m.m.), som I skal indsætte i jeres
  repository
- `catch-up/` – løsningen på modul 9 (`SparePartsController.cs`, `Mutation.cs`, `reservedele.js`). Kun hvis du
  ikke blev færdig med modul 9.

## Trin 0 – Kom i gang

Blev du ikke færdig med modul 9, skal du først kopiere indholdet af `catch-up/` ind over roden af dit repository.

1. Kopiér indholdet af `scaffolding/` ind over roden af dit repository. Den overskriver ingen af de filer, du
   skrev i modul 9.
   - Linux: `cp -R scaffolding/. ../dit-repository/`
   - Windows: åbn `scaffolding` i Stifinder, markér alt, og kopiér det ind i roden af dit repository.
2. Stop API'et, og slet `Peter-pedal-backend/src/PeterPedal.Api/peterpedal.db`. Databasen bliver kun oprettet,
   når filen ikke findes, og der er nye tabeller og data.
3. Start API'et og frontenden, hver i sin egen terminal, fra roden af dit repository:

       dotnet run --project Peter-pedal-backend/src/PeterPedal.Api
       dotnet run --project Peter-pedal-frontend/PeterPedal.Frontend

   API'et kører på http://localhost:5080 og frontenden på http://localhost:5100. I Swagger
   (http://localhost:5080/swagger) kan du nu se `POST /api/auth/login` og en **Authorize**-knap. Login svarer med
   fejl 500, indtil du har lavet opgave 1.

   **Genstart API'et efter hver ændring i C#-koden** (Ctrl+C i terminalen, og kør kommandoen igen). Ellers kører
   den gamle kode stadig. Ændringer i frontenden kræver ingen genstart: genindlæs siden i browseren med
   Ctrl+Shift+R, så browseren ikke bruger en gammel udgave af JavaScript-filerne.

   Scriptene `attack` (opgave 3) og `guess` (opgave 5) kører du i en tredje terminal, mens API'et kører.

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

Egon har sag 1, og Lise har sag 2.

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

## Opgave 1 – Log ind

For at API'et kan afgøre, hvem der må hvad, er det nødvendigt, at brugerne kan logge ind. Lav et
login-endpoint, `POST /api/auth/login`. Det tager imod et brugernavn og en adgangskode, og er de rigtige, giver
det brugeren et JWT. Tokenet skal fortælle, hvem brugeren er, og hvad brugeren må (scopes). Det må kun virke i
en begrænset tid.

**Krav**
- Hver rolle får kun de scopes, den har brug for (se tabellerne ovenfor). Ejeren må alt.
- Er login forkert, skal API'et svare med den korrekte HTTP-statuskode og en fejlbesked. Svaret må ikke
  afsløre, om det var brugernavnet eller adgangskoden, der var forkert.
- Er brugeren en kunde, skal tokenet også sige, hvilken kunde det er, i claim'en `customer_id`.

**Hints**
- Du arbejder i `Controllers/AuthController.cs`. Se `// TODO`.
- Brugerne ligger i databasen (`_db.Users`). Adgangskoderne er ikke gemt i klartekst, men
  `Auth/PasswordHasher.cs` kan tjekke dem.
- Indstillingerne til tokenet (nøgle, udsteder, modtager og levetid) står i `appsettings.json` under `Jwt`.
- Et JWT består af claims. Brug standard-claim'en for brugerens id (se introduktionen til JWT nedenfor) og `name` til
  brugerens navn.
- Scopes lægges i claims med navnet `scope`, én claim pr. scope.
- Tokenet laves med `JsonWebTokenHandler` fra `Microsoft.IdentityModel.JsonWebTokens`. Mange vejledninger
  bruger den ældre `JwtSecurityTokenHandler` – den virker også.

**Nyttige links**
- [JsonWebTokenHandler.CreateToken](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.jsonwebtokens.jsonwebtokenhandler.createtoken)
  og [SecurityTokenDescriptor](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.securitytokendescriptor)
  (Microsoft Learn)
- [Introduktion til JWT](https://jwt.io/introduction) (jwt.io)
- [HTTP-statuskoder](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) (MDN)
- [Fejlsvar i web-API'er](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors) (Microsoft Learn)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html),
  afsnittet "Authentication and Error Messages"

**Test:** Log ind som `frida` i Swagger. Kopiér tokenet ind på https://jwt.io, og find hendes id, navn, scopes
og udløbstidspunkt. Gør det samme for `lise`. Hvad er anderledes? Prøv også en forkert adgangskode og et
ukendt brugernavn. Hvilken statuskode får du, og hvorfor er det den rigtige?

## Opgave 2 – Authentication og authorization

Brugerne kan nu få et token, men API'et kigger ikke på det: alle kan stadig kalde alt. Prøv selv at kalde
`GET /api/customers` i Swagger uden at logge ind. For at beskytte Peter Pedals data skal API'et gøre to ting
ved hvert kald:

- **Authentication – hvem er du?** API'et tjekker, at tokenet er ægte, er lavet til Peter Pedal og ikke er
  udløbet.
- **Authorization – må du det her?** Hver handling kræver det scope, der passer til den (se scope-tabellen).

**Krav**
- Et kald uden et gyldigt token bliver afvist. Det gælder også et token, der er udløbet, er underskrevet med en
  anden nøgle eller er lavet til et andet system. Et udløbet token afvises med det samme – ikke først nogle
  minutter senere.
- Et kald med et gyldigt token, der mangler det rigtige scope, bliver også afvist, men med en anden statuskode.
- Både REST og GraphQL er beskyttet. En kunde må ikke se kunderne, reservedelene eller listen over alle sager,
  men skal stadig kunne spørge efter én sag (`repairCase`). Det kommer du tilbage til i opgave 3.

**Hints – authentication**
- Opsætningen er der allerede i `Program.cs`: *JWT bearer authentication*, én policy pr. scope, og begge dele
  slået til i pipelinen. Læs kommentarerne: hvorfor står hver linje der?
- Du bestemmer, hvad der tjekkes ved et token, med `TokenValidationParameters`. Se `// TODO` i `Program.cs`.
  Tokenet skal tjekkes mod præcis samme udsteder, modtager og nøgle, som du brugte, da du lavede det i opgave 1.
- Som standard godtager ASP.NET et token op til 5 minutter efter, det er udløbet, fordi urene på to servere kan
  gå forskelligt. Find indstillingen, der slår det fra.

**Hints – authorization**
- Hver policy har samme navn som sit scope, fx `Scopes.PartsWrite`.
- REST: sæt `[Authorize]` med den rigtige policy på controllerne og på de handlinger, der ændrer data.
- GraphQL: Hot Chocolate har sin **egen** `[Authorize]` i `HotChocolate.Authorization`. ASP.NET's `[Authorize]`
  på `Query` eller `Mutation` giver ingen fejl, men heller ingen beskyttelse. Lad ikke IDE'en vælge `using` for
  dig.
- Bruger du din egen `SparePartsController.cs` eller `Mutation.cs` fra modul 9, er der ingen `// TODO` i dem.
  Brug scope-tabellen til at finde ud af, hvad hver metode kræver.

**Nyttige links**
- [Introduktion til JWT](https://jwt.io/introduction) (jwt.io)
- [Configure JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
  (Microsoft Learn)
- [Policy-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
  (Microsoft Learn)
- [Authorization i Hot Chocolate](https://chillicream.com/docs/hotchocolate/v14/security/authorization)
- [HTTP-statuskoder](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) (MDN)
- [Svar og fejl i GraphQL](https://graphql.org/learn/response/) (graphql.org)

**Test:** Kald `GET /api/customers` i Swagger uden token. Hvilken statuskode får du? Log ind som `frida`, tryk
**Authorize**, og indsæt tokenet. Opret en kunde, og prøv at oprette en reservedel. Hvilke statuskoder får du
nu, og hvad er forskellen? I GraphQL (http://localhost:5080/graphql) sætter du headeren
`Authorization: Bearer <token>`. Prøv `addPart` som `frida` og som `bo`. Hvordan ser det ud, når GraphQL afviser
et kald? Log til sidst ind som `lise`: kan hun se kunderne, reservedelene eller listen over alle sager? Kan hun
stadig spørge efter sin egen sag?

## Opgave 3 – Authorization på objektniveau (BOLA)

Kunderne kan nu logge ind og få et token. Men et scope siger kun, *hvad* en bruger må gøre, ikke *hvilke data*
brugeren må se. Lise må gerne spørge efter en sag med `repairCase(id: ...)`, men kun sin egen. Prøv selv, hvad
der sker nu (API'et skal køre):

    dotnet run --project scripts/PeterPedal.Scripts -- attack

Lise logger ind og spørger efter sag 1, som er Egons. Hvad kan hun se?

Den fejl hedder **BOLA**, *Broken Object Level Authorization*: brugeren er logget ind og må bruge endpointet,
men API'et tjekker ikke, om netop det objekt (her sagen) er hendes. BOLA er nummer 1 på OWASP's liste over de
mest alvorlige sikkerhedsfejl i API'er. For at beskytte kundernes data skal API'et tjekke, at sagen tilhører den,
der spørger.

**Krav**
- En kunde kan kun læse sine egne sager.
- Medarbejdere kan stadig læse alle sager.
- Svaret på en andens sag må ikke afsløre, at sagen findes.

**Hints**
- Du arbejder i `GraphQL/Query.cs`, i `RepairCase`. Se `// TODO` – der står mere hjælp.
- Hvorfor kan du ikke løse det med en policy, som i opgave 2? Hvad ved du først, når sagen er hentet fra
  databasen?
- `RepairCase` får brugeren som `ClaimsPrincipal user`. Hvordan ser du, om brugeren er medarbejder, og hvilken
  kunde det er? Tænk på, hvad du lagde i tokenet i opgave 1.
- Hvad svarer API'et, når du spørger efter en sag, der ikke findes, fx `repairCase(id: 999)`?
- Tjekket skal ligge i API'et, ikke i frontenden. Angrebsscriptet kalder API'et direkte, uden om siden.
- `// TODO` nævner også logning. Det kan vente til opgave 6.

**Nyttige links**
- [OWASP API Security Top 10: API1 Broken Object Level Authorization](https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/)
- [ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal)
  (Microsoft Learn)

**Test:** Kør angrebet igen. Hvad ser Lise nu? Log ind som `lise`, og sammenlign i GraphQL svaret på
`repairCase(id: 1)` og `repairCase(id: 999)`. Er der nogen forskel? Kan hun stadig se sin egen sag 2? Log ind
som `frida`: kan hun stadig læse både sag 1 og sag 2? Hvorfor er det bedre at svare, som om sagen ikke findes, end
med en fejlbesked som "Det er ikke din sag"?

## Opgave 4 – Frontenden

Nu hvor API'et kræver login, virker frontenden ikke længere: den sender ikke noget token, så alle kald bliver
afvist. Prøv selv at åbne http://localhost:5100/sager.html. For at medarbejderne og kunderne kan bruge siderne
igen, skal brugeren kunne logge ind via login-siden. Frontenden skal gemme tokenet og sende det med hvert kald
til API'et.

**Krav**
- Login-siden (`login.html`) sender brugernavn og adgangskode til API'et. Ved forkert login viser siden en
  fejlbesked. Det samme sker, når API'et siger, at der har været for mange forsøg. Det kommer i opgave 5.
- Efter login lander en kunde på `min-sag.html` og en medarbejder på `sager.html`.
- Browseren husker tokenet, men kun så længe fanen er åben.
- Hvert kald til API'et, både REST og GraphQL, sender tokenet med.
- Er brugeren ikke logget ind, eller er tokenet udløbet, sendes brugeren til login-siden.

**Hints**
- Du arbejder i `js/auth.js`, `js/login.js`, `js/rest.js` og `js/graphql.js`. Se `// TODO`.
- Browseren har to slags lager: ét, der overlever, når fanen lukkes, og ét, der ikke gør. Hvilket passer til
  kravet, og hvorfor er det mere sikkert?
- Et JWT sendes normalt i en bestemt HTTP-header med ordet `Bearer` foran. Find ud af hvilken.
- Login-siden bruger `fetch` direkte og ikke `rest.js`. Her betyder en afvisning "forkert adgangskode" og ikke
  "gå til login-siden".
- REST og GraphQL siger "ikke logget ind" på hver sin måde. Kig på svarene fra testen i opgave 2.
- Hvordan ved login-siden, om brugeren er kunde? `readClaims` i `auth.js` læser indholdet af tokenet.

**Nyttige links**
- [Using the Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch) (MDN)
- [sessionStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage) (MDN)
- [Authorization-headeren](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization) (MDN)
- [Svar og fejl i GraphQL](https://graphql.org/learn/response/) (graphql.org)
- [HTTP-statuskoder](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) (MDN)
- [OWASP HTML5 Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/HTML5_Security_Cheat_Sheet.html),
  afsnittet "Local Storage"

**Test:** Log ind som `frida` på http://localhost:5100/login.html. Opret en kunde og en reservedel. Hvad sker
der med reservedelen, og hvorfor? Åbn udviklerværktøjerne (F12), og find tokenet i browserens lager og i et kald
under Network. Vent 2 minutter, og klik videre. Hvad sker der? Luk fanen, og åbn siden igen. Er du stadig logget
ind? Log til sidst ind som `lise`. Hvor lander du? Skriv sagsnummer 2, og ret så adressen fra
`min-sag.html?id=2` til `min-sag.html?id=1`. Hvad ser hun? Siden tjekker ikke selv, hvem sagen tilhører. Hvem
gør så?

## Opgave 5 – Rate limiting

Login-siden kan nu bruges af alle, også af en angriber, der prøver at gætte en adgangskode. En computer kan
prøve tusindvis af adgangskoder i minuttet. Prøv selv, hvad der sker nu (API'et skal køre):

    dotnet run --project scripts/PeterPedal.Scripts -- guess

Scriptet prøver 20 adgangskoder til Peters konto. Den rigtige er nr. 8. Hvor mange når det at prøve, og finder
det den rigtige?

For at gøre det langsomt og besværligt at gætte adgangskoder skal API'et begrænse, hvor mange login-forsøg der
må komme fra samme sted på kort tid.

**Krav**
- Hver IP-adresse må højst prøve at logge ind det antal gange pr. minut, der står i `appsettings.json` under
  `RateLimiting:LoginPermitsPerMinute`.
- Kommer der flere forsøg, afviser API'et dem med den statuskode, der betyder "for mange forespørgsler". Det
  gælder også, selv om adgangskoden er rigtig.
- Det er kun login, der begrænses. Resten af API'et virker som før.

**Hints**
- Du arbejder i `Program.cs` og i `Controllers/AuthController.cs`. Se `// TODO`.
- ASP.NET har en færdig *rate limiter* (se links). Der findes flere slags. Den simpleste tæller forsøg i faste
  tidsvinduer på et minut.
- Grænsen skal gælde pr. IP-adresse og ikke for alle brugere tilsammen. Ellers kan en angriber låse alle andre
  ude. Find ud af, hvordan man *partitionerer* en rate limiter.
- Giv din regel et navn, og brug navnet på `Login`. Som i opgave 2 skal den også slås til i pipelinen.

**Nyttige links**
- [Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
  (Microsoft Learn)
- [HTTP-statuskoder](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) (MDN)
- [OWASP: Brute force attack](https://owasp.org/www-community/attacks/Brute_force_attack)

**Test:** Kør `guess` igen. Hvilke statuskoder får du, og finder scriptet stadig den rigtige adgangskode? Har du
selv logget ind inden for det sidste minut, tæller de forsøg også med. Vent et minut, og prøv at logge ind på
login-siden. Virker det igen? Hvad ville der ske, hvis grænsen gjaldt for alle brugere tilsammen?

## Opgave 6 – Logging

API'et afviser nu forkerte logins, manglende tokens, forkerte scopes og andres sager. Men ingen kan se, at det
sker. Bliver Peter Pedal angrebet, skal man bagefter kunne finde ud af, hvad der skete: hvem prøvede hvad,
hvornår og hvorfra. For at kunne opdage og efterforske angreb skal API'et logge sikkerhedshændelserne.

**Krav**
- Disse hændelser bliver logget som advarsler:
  - et mislykket login
  - et kald, der bliver afvist, fordi tokenet mangler eller er ugyldigt
  - et kald, der bliver afvist, fordi scope'et mangler
  - en kunde, der prøver at læse en andens sag
- Hver linje fortæller nok til, at man kan efterforske hændelsen, fx hvem, hvad og hvorfra.
- Adgangskoder og tokens bliver **aldrig** logget. Hvorfor ikke?

**Hints**
- Mislykket login: i `Controllers/AuthController.cs`, der hvor du afviser login i opgave 1. Controlleren har
  allerede en `ILogger`.
- Afviste kald: i `Program.cs`. Se `// TODO`. JWT bearer har *events*, som ASP.NET kalder, når et kald bliver
  afvist. Der er ét for "ikke logget ind" og ét for "mangler rettigheder".
- Kundens forsøg: i `GraphQL/Query.cs`, der hvor du afviser sagen i opgave 3. `RepairCase` får allerede en
  `ILogger`.
- Brug pladsholdere i log-beskeden, fx `{Username}`, i stedet for at sætte teksten sammen selv.

**Nyttige links**
- [Logging in .NET and ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
  (Microsoft Learn)
- [JwtBearerEvents](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbearerevents)
  (Microsoft Learn)
- [HTTP-statuskoder](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) (MDN)
- [OWASP Logging Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html)

**Test:** Gentag testene fra opgave 1, 2 og 3, og kig i API'ets konsol. Kan du se, hvem der prøvede hvad? Står
der en adgangskode eller et token nogen steder? Bemærk: når GraphQL afviser et kald (`AUTH_NOT_AUTHORIZED`),
kommer der ingen linje. Hvorfor ikke?
