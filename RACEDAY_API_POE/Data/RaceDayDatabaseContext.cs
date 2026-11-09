using Microsoft.EntityFrameworkCore;
using RACEDAY_API_POE.Models;

namespace RACEDAY_API_POE.Data
{
    public class RaceDayDatabaseContext : DbContext
    {
        public RaceDayDatabaseContext(DbContextOptions<RaceDayDatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique Constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.EventId, e.CategoryId }).IsUnique();

            // Relationships
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
