using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;

namespace RoomService.Controllers;

[ApiController]
[Route("/rooms/{roomId}/slots")]
[Authorize]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly ILogger<SlotsController> _logger;

    public SlotsController(ISlotService slotService, ILogger<SlotsController> logger)
    {
        _slotService = slotService;
        _logger = logger;
    }

    /// <summary>
    /// Список доступных для бронирования слотов по переговорке и дате
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetAvailableSlots(Guid roomId, [FromQuery] string date)
    {
        // Валидация параметра date
        if (string.IsNullOrWhiteSpace(date))
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INVALID_REQUEST",
                    Message = "Date parameter is required"
                }
            });
        }

        if (!DateOnly.TryParse(date, out var parsedDate))
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INVALID_REQUEST",
                    Message = "Invalid date format. Use ISO format (YYYY-MM-DD)"
                }
            });
        }

        try
        {
            var slots = await _slotService.GetAvailableSlotsAsync(roomId, parsedDate);
            return Ok(slots);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "ROOM_NOT_FOUND",
                    Message = $"Room with id {roomId} not found"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slots for room {RoomId} on date {Date}", roomId, date);
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
