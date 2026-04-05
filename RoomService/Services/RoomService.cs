using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;


namespace RoomService.Services;
public class RoomsService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoomsService> _logger;

    public RoomsService(AppDbContext context, ILogger<RoomsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RoomResponse>> GetAllRoomsAsync()
    {
        var rooms = await _context.Rooms
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return rooms.Select(r => new RoomResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Capacity = r.Capacity,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<RoomResponse?> GetRoomByIdAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return null;

        return new RoomResponse
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt
        };
    }

    public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, Guid adminId)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} created room {RoomId} with name {RoomName}",
            adminId, room.Id, room.Name);

        return new RoomResponse
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt
        };
    }

    public async Task<bool> RoomExistsAsync(Guid id)
    {
        return await _context.Rooms.AnyAsync(r => r.Id == id);
    }
}