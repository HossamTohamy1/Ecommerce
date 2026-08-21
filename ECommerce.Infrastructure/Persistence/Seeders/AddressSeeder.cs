using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class AddressSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, List<ApplicationUser> customers, string actorId, ILogger logger)
    {
        var existingAddresses = await context.Addresses.ToListAsync();
        var existingUserIds = existingAddresses.Select(a => a.UserId).ToHashSet();

        var addressSpecs = new (string Street, string City, string Governorate, string PostalCode)[]
        {
            ("15 Tahrir Square, Downtown", "Cairo", "Cairo", "11511"),
            ("42 Lebanon St, Mohandessin", "Giza", "Giza", "12411"),
            ("88 Cornish El Nile, Stanley", "Alexandria", "Alexandria", "21523"),
            ("12 El Geish Street", "Mansoura", "Dakahlia", "35511"),
            ("25 Al Galaa Street", "Tanta", "Gharbia", "31511"),
            ("7 Corniche El Nil", "Aswan", "Aswan", "81511"),
            ("19 Port Said Street", "Ismailia", "Ismailia", "41511"),
            ("33 23rd of July Street", "Port Said", "Port Said", "42511"),
            ("10 Karnak Temple Road", "Luxor", "Luxor", "85951"),
            ("5 Orabi Street", "Zagazig", "Sharqia", "44511"),
            ("100 Sheraton Road", "Hurghada", "Red Sea", "84511"),
            ("22 Peace Road, Naama Bay", "Sharm El Sheikh", "South Sinai", "46619"),
            ("14 Gomhoria Street", "Damanhour", "Beheira", "22511"),
            ("9 Army Street", "Suez", "Suez", "43511"),
            ("18 Salah Salem Street", "Beni Suef", "Beni Suef", "62511"),
            ("27 Nile City Towers", "Cairo", "Cairo", "11221")
        };

        var toAdd = new List<Address>();
        for (int i = 0; i < customers.Count && i < addressSpecs.Length; i++)
        {
            var user = customers[i];
            if (existingUserIds.Contains(user.Id.ToString()))
            {
                continue;
            }

            var spec = addressSpecs[i];
            var addr = Address.Create(
                user.Id.ToString(),
                user.FullName,
                $"010{Random.Shared.Next(10000000, 99999999)}",
                spec.Street,
                spec.City,
                spec.Governorate,
                spec.PostalCode,
                true,
                actorId);
            toAdd.Add(addr);
        }

        if (toAdd.Count > 0)
        {
            context.Addresses.AddRange(toAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} new customer addresses", toAdd.Count);
        }
    }
}
