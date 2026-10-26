namespace PeterPedal.Api.Models;

// Someone who can log in: an employee, or a customer who may see their own cases.
// Never exposed through REST or GraphQL: it holds the password hash.
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = ""; // FrontDesk, Mechanic, Owner or Customer
    public string PasswordHash { get; set; } = "";
    public int? CustomerId { get; set; } // only for customers: which customer they are
}
