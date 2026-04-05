using RoomService.DTOs;

namespace RoomService.Services;
public interface IRoomService
{
    Task<List<RoomResponse>> GetAllRoomsAsync();
    Task<RoomResponse?> GetRoomByIdAsync(Guid id);
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, Guid adminId);
    Task<bool> RoomExistsAsync(Guid id);
}