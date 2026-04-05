using Microsoft.EntityFrameworkCore;
using RoomService.Models;

namespace RoomService.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.Migrate();

        // Добавляем фиксированных пользователей для dummyLogin
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Email = "admin@example.com",
                    Role = "admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Email = "user@example.com",
                    Role = "user",
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.SaveChanges();
        }

        // Добавляем тестовые комнаты если нужно
        if (!context.Rooms.Any())
        {
            context.Rooms.AddRange(
                new Room
                {
                    Id = Guid.NewGuid(),
                    Name = "Конференц-зал Alpha",
                    Description = "Большой зал с проектором, 20 мест",
                    Capacity = 20,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    Name = "Переговорка Beta",
                    Description = "Для маленьких команд, 6 мест",
                    Capacity = 6,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    Name = "VIP Зал",
                    Description = "Для важных встреч, 10 мест",
                    Capacity = 10,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.SaveChanges();
        }
    }
}