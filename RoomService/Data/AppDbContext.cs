using Microsoft.EntityFrameworkCore;
using RoomService.Models;

namespace RoomService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальный индекс: один активный слот не может иметь две активные брони
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.SlotId, b.Status })
            .HasDatabaseName("IX_Booking_SlotId_Status")
            .HasFilter("\"Status\" = 'active'");

        // Индекс для быстрого поиска слотов по комнате и дате
        modelBuilder.Entity<Slot>()
            .HasIndex(s => new { s.RoomId, s.Start })
            .IsUnique();

        // Уникальность расписания для комнаты
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => s.RoomId)
            .IsUnique();

        // Конвертация List<int> в JSONB для PostgreSQL
        modelBuilder.Entity<Schedule>()
            .Property(s => s.DaysOfWeek)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<int>()
            );
    }
}