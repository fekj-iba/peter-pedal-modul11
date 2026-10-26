using PeterPedal.Api.Auth;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.Data;

public static class SeedData
{
    public static void Seed(PeterPedalDbContext db)
    {
        // Any existing row means "not a fresh database" (e.g. Egon was deleted in a demo): do not re-seed.
        if (db.Customers.Any() || db.SpareParts.Any() || db.RepairCases.Any() || db.Users.Any())
        {
            return;
        }

        db.Users.AddRange(
            User("frida", "Frida", "FrontDesk", "frida123"),
            User("bo", "Bo", "Mechanic", "bo123"),
            User("anna", "Anna", "Mechanic", "anna123"),
            User("peter", "Peter", "Owner", "peter123"));

        var egon = new Customer { FirstName = "Egon", LastName = "Cykelmyggen", Phone = "20123456" };
        var lise = new Customer { FirstName = "Lise", LastName = "Hansen", Phone = "30987654" };

        db.SpareParts.AddRange(
            new SparePart { Name = "Gear cable", Price = 150m },
            new SparePart { Name = "Sprocket", Price = 300m },
            new SparePart { Name = "Brake pads", Price = 120m });

        db.RepairCases.Add(new RepairCase
        {
            FrameNumber = "STL-4471",
            Problem = "The gears are not shifting properly and the bike is almost impossible to ride.",
            Customer = egon
        });
        db.SaveChanges();

        db.RepairCases.Add(new RepairCase { FrameNumber = "KLM-2210", Problem = "Punktering på baghjulet.", Customer = lise });
        db.SaveChanges(); // separate save, so Egon and his case get id 1 and Lise and her case id 2

        // The customers' own logins, so they can follow their own cases.
        db.Users.AddRange(
            User("egon", "Egon", "Customer", "egon123", egon.Id),
            User("lise", "Lise", "Customer", "lise123", lise.Id));
        db.SaveChanges();
    }

    private static User User(string username, string name, string role, string password, int? customerId = null) =>
        new()
        {
            Username = username, Name = name, Role = role, PasswordHash = PasswordHasher.Hash(password),
            CustomerId = customerId
        };
}
