using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;

namespace RoomService.Controllers;

[ApiController]
[Route("/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Создать бронь на слот (только user)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        // Получаем userId из токена
        var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid token"
                }
            });
        }

        try
        {
            var booking = await _bookingService.CreateBookingAsync(userId, request);
            return StatusCode(201, new { booking });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "SLOT_NOT_FOUND",
                    Message = "Slot not found"
                }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("past"))
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INVALID_REQUEST",
                    Message = ex.Message
                }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already booked"))
        {
            return Conflict(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "SLOT_ALREADY_BOOKED",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Internal server error"
                }
            });
        }
    }

    /// <summary>
    /// Список всех броней с пагинацией (только admin)
    /// </summary>
    [HttpGet("list")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Валидация параметров
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var result = await _bookingService.GetAllBookingsAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Internal server error"
                }
            });
        }
    }

    /// <summary>
    /// Список броней текущего пользователя (только user)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid token"
                }
            });
        }

        try
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(new { bookings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Internal server error"
                }
            });
        }
    }

    /// <summary>
    /// Отменить бронь (только свою бронь, только user)
    /// </summary>
    [HttpPost("{bookingId}/cancel")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UNAUTHORIZED",
                    Message = "Invalid token"
                }
            });
        }

        try
        {
            var booking = await _bookingService.CancelBookingAsync(bookingId, userId);
            return Ok(new { booking });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "BOOKING_NOT_FOUND",
                    Message = $"Booking with id {bookingId} not found"
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "FORBIDDEN",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId} for user {UserId}", bookingId, userId);
            return StatusCode(500, new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Internal server error"
                }
            });
        }
    }
}
