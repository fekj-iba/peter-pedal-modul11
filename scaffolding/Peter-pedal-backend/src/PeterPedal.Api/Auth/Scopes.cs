namespace PeterPedal.Api.Auth;

// The scopes an access token can hold. Each scope is also the name of an authorization policy.
public static class Scopes
{
    public const string WorkshopRead = "workshop:read";     // read customers, spare parts and every case
    public const string CustomersWrite = "customers:write"; // POST/PUT /api/customers
    public const string PartsWrite = "parts:write";         // POST/PUT /api/spareparts
    public const string CasesCreate = "cases:create";       // createCase
    public const string CasesStatus = "cases:status";       // setStatus
    public const string CasesRepair = "cases:repair";       // addPart, calculateOffer
    public const string AdminDelete = "admin:delete";       // every delete

    public static readonly string[] All =
        { WorkshopRead, CustomersWrite, PartsWrite, CasesCreate, CasesStatus, CasesRepair, AdminDelete };
}
