using System;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unit = Domain.Unit;

namespace Infrastructure
{

    public class FridgeAppContext: DbContext
    {
        private readonly bool _log;

        public FridgeAppContext()
        {
        }

        public FridgeAppContext(bool log)
        {
            _log = log;
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Unit> Units { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=127.0.0.1;Database=FFDatabase1;User=sa;Password=@AdminPassWord123;Trust Server Certificate=True");
            //optionsBuilder.UseSqlServer(@"Server=localhost;Database=TestServerApp;User=sa;Password=@AdminPassWord123;Trust Server Certificate=True");
            if (_log)
            {
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasData((new Item
                { Id = 1, Name = "Tomatoes", UnitDesc = "Per Tomato" }));
            modelBuilder.Entity<Unit>().HasData((new Unit
                { Id = 1, Quantity = 3, ExpiryDate = new DateTime(2022, 03, 11), ItemId = 1}));
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