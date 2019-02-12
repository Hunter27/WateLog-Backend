using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend.Models
{
    public class DatabaseContext : DbContext

    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        public DatabaseContext() { }
        public DbSet<Location> Location { get; set; }
        public DbSet<Monitor> Monitor { get; set; }
        public DbSet<Reading> Reading { get; set; }
        public DbSet<SegmentEvent> SegmentEvent { get; set; }
        public DbSet<Segment> Segment { get; set; }
        public DbSet<ActionableEvent> ActionableEvent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Monitor>()
                .HasMany(c => c.Reading)
                .WithOne(e => e.Monitor)
                .HasForeignKey(d => d.MonitorId);

            modelBuilder.Entity<Location>()
                 .HasOne(a => a.Monitor)
                 .WithOne(b => b.Location)
                 .HasForeignKey<Location>(c => c.MonitorId);

            modelBuilder.Entity<Segment>()
                .HasOne(a => a.Monitor)
                .WithOne(b => b.Segment)
                .HasForeignKey<Segment>(c => c.Monitor1Id);

            modelBuilder.Entity<Segment>()
                .HasOne(a => a.Monitor)
                .WithOne(b => b.Segment)
                .HasForeignKey<Segment>(c => c.Monitor2Id);

            modelBuilder.Entity<SegmentEvent>()
                .HasOne(a => a.Segment)
                .WithMany(b => b.SegmentEvent)
                .HasForeignKey(c => c.SegmentId);

            modelBuilder.Entity<ActionableEvent>()
                .HasOne(a => a.Segment)
                .WithMany(b => b.ActionableEvent)
                .HasForeignKey(c => c.SegmentId);
        }
        
    }
}
