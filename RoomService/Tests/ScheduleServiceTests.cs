using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Services;
using Xunit;

namespace RoomService.Tests;
public class ScheduleServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateScheduleAsync_ShouldCreateSchedule_WhenValidRequest()
    {
        // Arrange
        var context = GetDbContext();
        var roomService = new RoomsService(context, Mock.Of<ILogger<RoomsService>>());
        var scheduleService = new ScheduleService(context, roomService, Mock.Of<ILogger<ScheduleService>>());

        var adminId = Guid.NewGuid();
        var room = await roomService.CreateRoomAsync(new CreateRoomRequest { Name = "Test Room" }, adminId);

        var request = new CreateScheduleRequest
        {
            DaysOfWeek = new List<int> { 1, 2, 3, 4, 5 },
            StartTime = "09:00",
            EndTime = "18:00"
        };

        // Act
        var result = await scheduleService.CreateScheduleAsync(room.Id, request, adminId);

        // Assert
        result.Should().NotBeNull();
        result.RoomId.Should().Be(room.Id);
        result.DaysOfWeek.Should().Contain(new[] { 1, 2, 3, 4, 5 });
        result.StartTime.Should().Be("09:00");
        result.EndTime.Should().Be("18:00");
    }

    [Fact]
    public async Task CreateScheduleAsync_ShouldThrowException_WhenScheduleAlreadyExists()
    {
        // Arrange
        var context = GetDbContext();
        var roomService = new RoomsService(context, Mock.Of<ILogger<RoomsService>>());
        var scheduleService = new ScheduleService(context, roomService, Mock.Of<ILogger<ScheduleService>>());

        var adminId = Guid.NewGuid();
        var room = await roomService.CreateRoomAsync(new CreateRoomRequest { Name = "Test Room" }, adminId);

        var request = new CreateScheduleRequest
        {
            DaysOfWeek = new List<int> { 1, 2, 3 },
            StartTime = "09:00",
            EndTime = "17:00"
        };

        await scheduleService.CreateScheduleAsync(room.Id, request, adminId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scheduleService.CreateScheduleAsync(room.Id, request, adminId));
    }
}