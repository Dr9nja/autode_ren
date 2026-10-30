using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.Application.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Shop> Shops { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CarOrder> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RentalUser> RentalUsers { get; set; }
        public DbSet<RentalCar> RentalCars { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarOrder>()
                .HasOne(order => order.Payment)
                .WithOne(payment => payment.Order)
                .HasForeignKey<Payment>(payment => payment.OrderId);

            modelBuilder.Entity<CarOrder>()
                .HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId);

            modelBuilder.Entity<Customer>()
                .HasOne(customer => customer.Address)
                .WithOne(address => address.Customer)
                .HasForeignKey<Customer>(customer => customer.AddressId);

            modelBuilder.Entity<CarOrder>()
                .HasMany(order => order.Notifications)
                .WithOne(notification => notification.Order)
                .HasForeignKey(notification => notification.OrderId);

            modelBuilder.Entity<Rental>()
                .HasIndex(rental => rental.CarId)
                .IsUnique()
                .HasFilter("[Status] = 'active'");

            modelBuilder.Entity<Rental>()
                .HasOne(rental => rental.Invoice)
                .WithOne(invoice => invoice.Rental)
                .HasForeignKey<Invoice>(invoice => invoice.RentalId);

            modelBuilder.Entity<Invoice>()
                .HasOne(invoice => invoice.Line)
                .WithOne(line => line.Invoice)
                .HasForeignKey<InvoiceLine>(line => line.InvoiceId);

            modelBuilder.Entity<RentalCar>()
                .Property(car => car.TimeRatePerMinute)
                .HasPrecision(10, 4);

            modelBuilder.Entity<RentalCar>()
                .Property(car => car.KilometerRate)
                .HasPrecision(10, 3);

            modelBuilder.Entity<Rental>()
                .Property(rental => rental.KilometersDriven)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceLine>()
                .Property(line => line.Amount)
                .HasPrecision(18, 2);
        }
    }
}
