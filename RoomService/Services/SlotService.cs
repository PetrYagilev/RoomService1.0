using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services;
public class SlotService : ISlotService
{
    private readonly AppDbContext _context;
    private readonly IRoomService _roomService;
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<SlotService> _logger;

    // Длительность слота 30 минут
    private static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(30);

    public SlotService(
        AppDbContext context,
        IRoomService roomService,
        IScheduleService scheduleService,
        ILogger<SlotService> logger)
    {
        _context = context;
        _roomService = roomService;
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task<SlotsListResponse> GetAvailableSlotsAsync(Guid roomId, DateOnly date)
    {
        // 1. Проверяем существование комнаты
        if (!await _roomService.RoomExistsAsync(roomId))
        {
            throw new KeyNotFoundException($"Room with id {roomId} not found");
        }

        // 2. Получаем расписание комнаты
        var schedule = await _scheduleService.GetScheduleByRoomIdAsync(roomId);
        if (schedule == null)
        {
            // Нет расписания - комната всегда недоступна
            return new SlotsListResponse { Slots = new List<SlotResponse>() };
        }

        // 3. Проверяем, доступна ли комната в этот день недели
        var dayOfWeek = GetDayOfWeekNumber(date);
        if (!schedule.DaysOfWeek.Contains(dayOfWeek))
        {
            return new SlotsListResponse { Slots = new List<SlotResponse>() };
        }

        // 4. Парсим время начала и окончания
        var startTime = TimeOnly.Parse(schedule.StartTime);
        var endTime = TimeOnly.Parse(schedule.EndTime);

        // 5. Генерируем все возможные слоты для этого дня
        var allSlots = GenerateSlotsForDay(roomId, date, startTime, endTime);

        if (!allSlots.Any())
        {
            return new SlotsListResponse { Slots = new List<SlotResponse>() };
        }

        // 6. Получаем занятые слоты (с активными бронями)
        var slotIds = allSlots.Select(s => s.Id).ToList();
        var bookedSlotIds = await _context.Bookings
            .Where(b => slotIds.Contains(b.SlotId) && b.Status == "active")
            .Select(b => b.SlotId)
            .ToListAsync();

        // 7. Фильтруем только свободные слоты
        var availableSlots = allSlots
            .Where(s => !bookedSlotIds.Contains(s.Id))
            .Select(s => new SlotResponse
            {
                Id = s.Id,
                RoomId = s.RoomId,
                Start = s.Start,
                End = s.End
            })
            .OrderBy(s => s.Start)
            .ToList();

        return new SlotsListResponse { Slots = availableSlots };
    }

    private List<Slot> GenerateSlotsForDay(Guid roomId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var slots = new List<Slot>();
        var currentTime = startTime;
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        while (currentTime < endTime)
        {
            var slotStart = dateTime + currentTime.ToTimeSpan();
            var slotEnd = slotStart + SlotDuration;

            // Проверяем, что слот не выходит за пределы рабочего времени
            if (slotEnd <= dateTime + endTime.ToTimeSpan())
            {
                // Генерируем детерминированный UUID для слота
                // Это важно для стабильности и повторяемости
                var slotId = GenerateDeterministicSlotId(roomId, slotStart);

                slots.Add(new Slot
                {
                    Id = slotId,
                    RoomId = roomId,
                    Start = slotStart,
                    End = slotEnd
                });
            }

            currentTime = currentTime.Add(SlotDuration);
        }

        return slots;
    }

    private Guid GenerateDeterministicSlotId(Guid roomId, DateTime slotStart)
    {
        // Создаем детерминированный UUID на основе roomId и времени начала слота
        // Это гарантирует, что для одного и того же слота всегда будет один ID
        var hashInput = $"{roomId}_{slotStart:yyyyMMddHHmmss}";
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashInput));
        return new Guid(hash);
    }

    private int GetDayOfWeekNumber(DateOnly date)
    {
        // Конвертируем DayOfWeek (Sunday=0) в наш формат (Monday=1, Sunday=7)
        var dotNetDay = date.DayOfWeek;
        return dotNetDay == DayOfWeek.Sunday ? 7 : (int)dotNetDay;
    }
}