using System;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace Infrastructure
{

    public class AppContext: DbContext
    {
        private readonly bool _log;

        public AppContext()
        {
        }

        public AppContext(bool log)
        {
            _log = log;
        }

        public DbSet<Fridge> Fridges { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Unit> Units { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=127.0.0.1;Database=FFDatabase1;User=sa;Password=@AdminPassWord123;Trust Server Certificate=True");

            if (_log)
            {
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            // modelBuilder.Entity<Person>()
            //     .HasOne(p => p.Team);
            // modelBuilder
            //     .Entity<Application>()
            //     .ToTable("Applications", b => b.IsTemporal());
            // modelBuilder
            //     .Entity<Person>()
            //     .ToTable("People", b => b.IsTemporal());
            // modelBuilder
            //     .Entity<Team>()
            //     .ToTable("Teams", b => b.IsTemporal());
            
                
        }
    }
}