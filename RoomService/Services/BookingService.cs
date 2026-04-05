using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.DTOs;
using RoomService.Models;

namespace RoomService.Services;
public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookingService> _logger;
    private readonly IConferenceLinkService _conferenceLinkService;

    public BookingService(
        AppDbContext context,
        ILogger<BookingService> logger,
        IConferenceLinkService conferenceLinkService)
    {
        _context = context;
        _logger = logger;
        _conferenceLinkService = conferenceLinkService;
    }

    public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request)
    {
        // 1. Проверяем существование слота
        var slot = await _context.Slots.FindAsync(request.SlotId);
        if (slot == null)
        {
            throw new KeyNotFoundException($"Slot with id {request.SlotId} not found");
        }

        // 2. Проверяем, что слот не в прошлом
        if (slot.Start < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot book a slot in the past");
        }

        // 3. Проверяем, что слот ещё не занят
        var existingBooking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.SlotId == request.SlotId && b.Status == "active");

        if (existingBooking != null)
        {
            throw new InvalidOperationException("Slot is already booked");
        }

        // 4. Создаём бронь
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            SlotId = request.SlotId,
            UserId = userId,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        // 5. Если запрошена ссылка на конференцию
        if (request.CreateConferenceLink)
        {
            try
            {
                booking.ConferenceLink = await _conferenceLinkService.CreateConferenceLinkAsync(booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create conference link for booking {BookingId}", booking.Id);
                // Продолжаем без ссылки на конференцию
            }
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created booking {BookingId} for slot {SlotId}",
            userId, booking.Id, request.SlotId);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelBookingAsync(Guid bookingId, Guid userId)
    {
        // 1. Находим бронь
        var booking = await _context.Bookings
            .Include(b => b.Slot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new KeyNotFoundException($"Booking with id {bookingId} not found");
        }

        // 2. Проверяем, что пользователь - владелец брони
        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("Cannot cancel another user's booking");
        }

        // 3. Если уже отменена, просто возвращаем текущее состояние (идемпотентность)
        if (booking.Status == "cancelled")
        {
            return MapToResponse(booking);
        }

        // 4. Отменяем бронь
        booking.Status = "cancelled";
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} cancelled booking {BookingId}", userId, bookingId);

        return MapToResponse(booking);
    }

    public async Task<BookingsListResponse> GetAllBookingsAsync(int page, int pageSize)
    {
        var query = _context.Bookings
            .Include(b => b.Slot)
            .OrderByDescending(b => b.CreatedAt);

        var total = await query.CountAsync();
        var bookings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new BookingsListResponse
        {
            Bookings = bookings.Select(MapToResponse).ToList(),
            Pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total
            }
        };
    }

    public async Task<List<BookingResponse>> GetUserBookingsAsync(Guid userId)
    {
        // Только будущие слоты (start >= now)
        var now = DateTime.UtcNow;

        var bookings = await _context.Bookings
            .Include(b => b.Slot)
            .Where(b => b.UserId == userId && b.Status == "active" && b.Slot != null && b.Slot.Start >= now)
            .OrderBy(b => b.Slot!.Start)
            .ToListAsync();

        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<bool> IsSlotBookedAsync(Guid slotId)
    {
        return await _context.Bookings
            .AnyAsync(b => b.SlotId == slotId && b.Status == "active");
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Slot)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking != null ? MapToResponse(booking) : null;
    }

    private BookingResponse MapToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            SlotId = booking.SlotId,
            UserId = booking.UserId,
            Status = booking.Status,
            ConferenceLink = booking.ConferenceLink,
            CreatedAt = booking.CreatedAt
        };
    }
}
