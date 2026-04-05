using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Services;
using Xunit;

namespace RoomService.Tests;
public class RoomServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldCreateRoom_WhenValidRequest()
    {
        // Arrange
        var context = GetDbContext();
        var logger = Mock.Of<ILogger<RoomsService>>();
        var service = new RoomsService(context, logger);
        var adminId = Guid.NewGuid();
        var request = new CreateRoomRequest
        {
            Name = "Test Room",
            Description = "Test Description",
            Capacity = 10
        };

        // Act
        var result = await service.CreateRoomAsync(request, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Room");
        result.Capacity.Should().Be(10);

        var savedRoom = await context.Rooms.FindAsync(result.Id);
        savedRoom.Should().NotBeNull();
        savedRoom.Name.Should().Be("Test Room");
    }

    [Fact]
    public async Task GetAllRoomsAsync_ShouldReturnAllRooms()
    {
        // Arrange
        var context = GetDbContext();
        var logger = Mock.Of<ILogger<RoomsService>>();
        var service = new RoomsService(context, logger);

        var adminId = Guid.NewGuid();
        await service.CreateRoomAsync(new CreateRoomRequest { Name = "Room 1" }, adminId);
        await service.CreateRoomAsync(new CreateRoomRequest { Name = "Room 2" }, adminId);

        // Act
        var result = await service.GetAllRoomsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().Contain("Room 1");
        result.Select(r => r.Name).Should().Contain("Room 2");
    }

    [Fact]
    public async Task RoomExistsAsync_ShouldReturnTrue_WhenRoomExists()
    {
        // Arrange
        var context = GetDbContext();
        var logger = Mock.Of<ILogger<RoomsService>>();
        var service = new RoomsService(context, logger);
        var adminId = Guid.NewGuid();
        var room = await service.CreateRoomAsync(new CreateRoomRequest { Name = "Test" }, adminId);

        // Act
        var exists = await service.RoomExistsAsync(room.Id);

        // Assert
        exists.Should().BeTrue();
    }
}
