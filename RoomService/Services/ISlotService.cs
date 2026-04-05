using RoomService.DTOs;


namespace RoomService.Services;
public interface ISlotService
{
    Task<SlotsListResponse> GetAvailableSlotsAsync(Guid roomId, DateOnly date);
}