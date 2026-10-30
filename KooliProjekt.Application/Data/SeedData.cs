using System.Linq;

namespace KooliProjekt.Application.Data
{
    public static class SeedData
    {
        public static void Generate(ApplicationDbContext context)
        {
            if (context.Shops.Any())
            {
                return;
            }

            context.Shops.Add(new Shop
            {
                Name = "Auto Deren",
                Street = "Main Street 1",
                Email = "hello@autoderen.example",
                Country = "Estonia"
            });
            context.SaveChanges();
        }
    }
}
