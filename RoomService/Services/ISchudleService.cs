using RoomService.DTOs;

namespace RoomService.Services;
public interface IScheduleService
{
    Task<ScheduleResponse> CreateScheduleAsync(Guid roomId, CreateScheduleRequest request, Guid adminId);
    Task<bool> HasScheduleAsync(Guid roomId);
    Task<ScheduleResponse?> GetScheduleByRoomIdAsync(Guid roomId);
}
