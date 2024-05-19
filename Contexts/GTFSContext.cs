using System.Data;
using System.Text.Json;
using GTFS.Entities;
using Microsoft.EntityFrameworkCore;
using Route = GTFS.Entities.Route;

namespace komikaan.Harvester.Contexts;

internal class GTFSContext : DbContext
{

    public DbSet<Agency> Agencies { get; set; }
    public DbSet<Stop> Stops { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<CalendarDate> CalendarDates { get; set; }
    public DbSet<Frequency> Frequencies { get; set; }
    public DbSet<StopTime> StopTimes { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Shape> Shapes { get; set; }

    public GTFSContext(DbContextOptions<GTFSContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Frequency>().HasNoKey();
        modelBuilder.Entity<StopTime>()
            .Property(stopTime => stopTime.ArrivalTime)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<TimeOfDay>(v, (JsonSerializerOptions)null)); modelBuilder.Entity<StopTime>()
            .Property(stopTime => stopTime.DepartureTime)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<TimeOfDay>(v, (JsonSerializerOptions)null));

    }

    protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
}