namespace RoomService.Services;
public interface IConferenceLinkService
{
    Task<string> CreateConferenceLinkAsync(Guid bookingId);
}