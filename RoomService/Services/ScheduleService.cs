using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services;
public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _context;
    private readonly IRoomService _roomService;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(AppDbContext context, IRoomService roomService, ILogger<ScheduleService> logger)
    {
        _context = context;
        _roomService = roomService;
        _logger = logger;
    }

    public async Task<ScheduleResponse> CreateScheduleAsync(Guid roomId, CreateScheduleRequest request, Guid adminId)
    {
        // Проверяем существование комнаты
        if (!await _roomService.RoomExistsAsync(roomId))
        {
            throw new KeyNotFoundException($"Room with id {roomId} not found");
        }

        // Проверяем, нет ли уже расписания
        if (await HasScheduleAsync(roomId))
        {
            throw new InvalidOperationException("Schedule for this room already exists and cannot be changed");
        }

        // Валидация дней недели
        foreach (var day in request.DaysOfWeek)
        {
            if (day < 1 || day > 7)
            {
                throw new ArgumentException($"Invalid day of week: {day}. Must be between 1 and 7");
            }
        }

        // Убираем дубликаты
        var uniqueDays = request.DaysOfWeek.Distinct().OrderBy(d => d).ToList();

        // Парсим время
        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            throw new ArgumentException("Invalid time format");
        }

        // Проверяем, что время начала меньше времени окончания
        if (startTime >= endTime)
        {
            throw new ArgumentException("Start time must be less than end time");
        }

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            DaysOfWeek = uniqueDays,
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} created schedule for room {RoomId}", adminId, roomId);

        return new ScheduleResponse
        {
            Id = schedule.Id,
            RoomId = schedule.RoomId,
            DaysOfWeek = schedule.DaysOfWeek,
            StartTime = schedule.StartTime.ToString("HH:mm"),
            EndTime = schedule.EndTime.ToString("HH:mm")
        };
    }

    public async Task<bool> HasScheduleAsync(Guid roomId)
    {
        return await _context.Schedules.AnyAsync(s => s.RoomId == roomId);
    }

    public async Task<ScheduleResponse?> GetScheduleByRoomIdAsync(Guid roomId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (schedule == null) return null;

        return new ScheduleResponse
        {
            Id = schedule.Id,
            RoomId = schedule.RoomId,
            DaysOfWeek = schedule.DaysOfWeek,
            StartTime = schedule.StartTime.ToString("HH:mm"),
            EndTime = schedule.EndTime.ToString("HH:mm")
        };
    }
}