using DoanCStar.Models;
using Microsoft.EntityFrameworkCore;
namespace DoanCStar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<Concession> Concessions { get; set; }
        public DbSet<BookingConcession> BookingConcessions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<SnackOrder> SnackOrders { get; set; }
        public DbSet<SnackOrderDetail> SnackOrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookingSeat>()
        .HasOne(bs => bs.Seat)
        .WithMany()
        .HasForeignKey(bs => bs.SeatId)
        .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Concession>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BookingConcession>()
                .Property(bc => bc.Subtotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BookingSeat>()
                .Property(bs => bs.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountPercent)
                .HasColumnType("decimal(18,2)");

         
        }
    }

}
