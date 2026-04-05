using RoomService.DTOs;

namespace RoomService.Services;
public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request);
    Task<BookingResponse> CancelBookingAsync(Guid bookingId, Guid userId);
    Task<BookingsListResponse> GetAllBookingsAsync(int page, int pageSize);
    Task<List<BookingResponse>> GetUserBookingsAsync(Guid userId);
    Task<bool> IsSlotBookedAsync(Guid slotId);
    Task<BookingResponse?> GetBookingByIdAsync(Guid bookingId);
}