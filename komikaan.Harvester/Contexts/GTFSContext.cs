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
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.Entity<Frequency>();
        modelBuilder.Entity<Shape>();
        modelBuilder.Entity<Stop>();
        modelBuilder.Entity<Agency>();

        modelBuilder.Entity<Calendar>();

        modelBuilder.Entity<Stop>();

        modelBuilder.Entity<Pathway>();

        modelBuilder.Entity<Route>();

        modelBuilder.Entity<StopTime>();

        modelBuilder.Entity<Transfer>();

        modelBuilder.Entity<CalendarDate>();

        modelBuilder.Entity<Trip>();

    }

    protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
}