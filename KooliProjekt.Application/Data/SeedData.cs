using System.Linq;
using System.Collections.Generic;

namespace KooliProjekt.Application.Data
{
    public static class SeedData
    {
        public static void Generate(ApplicationDbContext context)
        {
            if (!context.Shops.Any())
            {
                context.Shops.Add(new Shop
                {
                    Name = "Auto Deren",
                    Street = "Main Street 1",
                    Email = "hello@autoderen.example",
                    Country = "Estonia"
                });
            }

            if (!context.RentalCars.Any())
            {
                context.RentalCars.AddRange(CreateInitialFleet());
            }

            context.SaveChanges();
        }

        private static IEnumerable<RentalCar> CreateInitialFleet()
        {
            var cars = new[]
            {
                ("Economy", "Toyota Yaris"),
                ("Economy", "Volkswagen Polo"),
                ("Economy", "Skoda Fabia"),
                ("Economy", "Honda Jazz"),
                ("Economy", "Hyundai i20"),
                ("Economy", "Kia Rio"),
                ("Economy", "Renault Clio"),
                ("Family", "Skoda Octavia"),
                ("Family", "Toyota Corolla"),
                ("Family", "Volkswagen Passat"),
                ("Family", "Ford Focus"),
                ("Family", "Kia Ceed"),
                ("Family", "Hyundai Tucson"),
                ("Family", "Toyota RAV4"),
                ("Electric", "Nissan Leaf"),
                ("Electric", "Tesla Model 3"),
                ("Electric", "Hyundai Kona Electric"),
                ("Electric", "Volkswagen ID.3"),
                ("Electric", "Kia Niro EV"),
                ("Electric", "Skoda Enyaq")
            };

            return cars.Select((car, index) => new RentalCar
            {
                RegistrationNumber = $"RENT-{index + 1:000}",
                Type = car.Item1,
                Model = car.Item2,
                TimeRatePerMinute = 0.025m + index * 0.001m,
                KilometerRate = 0.18m + index * 0.005m
            });
        }
    }
}
