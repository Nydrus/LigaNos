using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using static iText.IO.Util.IntHashtable;

namespace LigaNOS.Data
{

    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Stat> Stats { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Contact> Contacts { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.LogTo(Console.WriteLine);
                     
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Club>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.ImageData);
                
                    

                entity.Property(c => c.ImageTitle)
                    .HasMaxLength(50)
                    .IsUnicode();

                entity.Property(c => c.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(c => c.Coach)
                    .HasMaxLength(50)
                    .IsRequired();

                
            });

            modelBuilder.Entity<Match>()
                        .HasOne(m => m.HomeClub)
                        .WithMany(c => c.HomeMatches)
                        .HasForeignKey(m => new { m.HomeClubId})
                        .OnDelete(DeleteBehavior.NoAction); 
 
            modelBuilder.Entity<Match>()
                        .HasOne(m => m.AwayClub)
                        .WithMany(c => c.AwayMatches)
                        .HasForeignKey(m => new { m.AwayClubId})
                        .HasPrincipalKey(c => new { c.Id })
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Stat>()
                        .HasOne(s => s.HomeClub)
                        .WithMany(c => c.HomeStats)  
                        .HasForeignKey(s => s.HomeClubId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stat>()
                        .HasOne(s => s.AwayClub)
                        .WithMany(c => c.AwayStats)  
                        .HasForeignKey(s => s.AwayClubId)
                        .OnDelete(DeleteBehavior.Restrict);
           
            modelBuilder.Entity<Player>()
                        .HasOne(p => p.Club)  
                        .WithMany(c => c.Players)  
                        .HasForeignKey(p => new { p.ClubId})
                        .HasPrincipalKey(c => new { c.Id})
                        .OnDelete(DeleteBehavior.SetNull);
            

        }
    }
}
