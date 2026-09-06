using Ataa.Models;
using Microsoft.EntityFrameworkCore;

namespace Ataa.Data
{
    public class AtaaDbContext : DbContext
    {
        public AtaaDbContext(DbContextOptions<AtaaDbContext> options) : base(options)
        {
        }

        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Volunteers> Volunteers { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Events> Events { get; set; }
   
        public DbSet<VolunteerSkills> VolunteerSkills { get; set; }
        public DbSet<VolunteerInterests> VolunteerInterests { get; set; }
        public DbSet<EventSkills> EventSkills { get; set; }
        public DbSet<EventInterests> EventInterests { get; set; }
        public DbSet<EventRegistrations> EventRegistrations { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<Interests> Interests { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Donors> Donors { get; set; }
        public DbSet<Donations> Donations { get; set; }
        public DbSet<DonationItems> DonationItems { get; set; }
        public DbSet<Certificates> Certificates { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Volunteers>()
                .HasIndex(v => v.Email) 
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email) 
                .IsUnique();
        }
    }
}