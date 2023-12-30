using Microsoft.EntityFrameworkCore;

namespace RaceCondition
{
    public class ReservationDbContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketWithVersion> TicketWithVersion { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Initial Catalog=Reservation; Integrated Security=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>().HasData(
                new Ticket { Id = 1, Name = "Concert Ticket", Available = true }
            );

            modelBuilder.Entity<TicketWithVersion>().HasData(
                new TicketWithVersion { Id = 1, Name = "Concert Ticket", Available = true }
            );
        }
    }
}
