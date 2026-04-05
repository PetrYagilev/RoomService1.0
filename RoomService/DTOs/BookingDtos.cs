namespace RoomService.DTOs;
public class CreateBookingRequest
{
    public Guid SlotId { get; set; }
    public bool CreateConferenceLink { get; set; } = false;
}

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ConferenceLink { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingsListResponse
{
    public List<BookingResponse> Bookings { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class CancelBookingResponse
{
    public BookingResponse Booking { get; set; } = null!;
}