using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Services;
using Xunit;

namespace RoomService.Tests;

public class SlotServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ShouldReturnSlots_WhenScheduleExists()
    {
        // Arrange
        var context = GetDbContext();
        var roomService = new RoomsService(context, Mock.Of<ILogger<RoomsService>>());
        var scheduleService = new ScheduleService(context, roomService, Mock.Of<ILogger<ScheduleService>>());
        var slotService = new SlotService(context, roomService, scheduleService, Mock.Of<ILogger<SlotService>>());

        var adminId = Guid.NewGuid();
        var room = await roomService.CreateRoomAsync(new CreateRoomRequest { Name = "Test Room" }, adminId);

        var scheduleRequest = new CreateScheduleRequest
        {
            DaysOfWeek = new List<int> { 1 }, // Monday
            StartTime = "09:00",
            EndTime = "10:00" // 1 hour = 2 slots of 30 min
        };

        await scheduleService.CreateScheduleAsync(room.Id, scheduleRequest, adminId);

        // Get next Monday
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntilMonday = ((1 - (int)today.DayOfWeek + 7) % 7);
        var nextMonday = today.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);

        // Act
        var result = await slotService.GetAvailableSlotsAsync(room.Id, nextMonday);

        // Assert
        result.Slots.Should().HaveCount(2); // 09:00-09:30 and 09:30-10:00
        result.Slots[0].Start.TimeOfDay.Should().Be(TimeSpan.FromHours(9));
        result.Slots[0].End.TimeOfDay.Should().Be(TimeSpan.FromHours(9.5));
        result.Slots[1].Start.TimeOfDay.Should().Be(TimeSpan.FromHours(9.5));
        result.Slots[1].End.TimeOfDay.Should().Be(TimeSpan.FromHours(10));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ShouldReturnEmpty_WhenNoSchedule()
    {
        // Arrange
        var context = GetDbContext();
        var roomService = new RoomsService(context, Mock.Of<ILogger<RoomsService>>());
        var scheduleService = new ScheduleService(context, roomService, Mock.Of<ILogger<ScheduleService>>());
        var slotService = new SlotService(context, roomService, scheduleService, Mock.Of<ILogger<SlotService>>());

        var adminId = Guid.NewGuid();
        var room = await roomService.CreateRoomAsync(new CreateRoomRequest { Name = "Test Room" }, adminId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await slotService.GetAvailableSlotsAsync(room.Id, today);

        // Assert
        result.Slots.Should().BeEmpty();
    }
}
