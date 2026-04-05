using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;


namespace RoomService.Controllers;

[ApiController]
[Route("/rooms/{roomId}/schedule")]
[Authorize(Roles = "admin")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(IScheduleService scheduleService, ILogger<SchedulesController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    /// <summary>
    /// Создать расписание переговорки (только admin, только один раз)
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateSchedule(Guid roomId, [FromBody] CreateScheduleRequest request)
    {
        try
        {
            // Получаем adminId из токена
            var adminIdClaim = User.FindFirst("user_id");
            if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out var adminId))
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

            var schedule = await _scheduleService.CreateScheduleAsync(roomId, request, adminId);
            return StatusCode(201, new { schedule });
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "SCHEDULE_EXISTS",
                    Message = ex.Message
                }
            });
        }
        catch (ArgumentException ex)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule for room {RoomId}", roomId);
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
