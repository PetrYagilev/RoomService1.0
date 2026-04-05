using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Services;
using System.Security.Claims;

namespace RoomService.Controllers;

[ApiController]
[Route("/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Список переговорок (admin и user)
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetRooms()
    {
        try
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(new { rooms });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rooms");
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
    /// Создать переговорку (только admin)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INVALID_REQUEST",
                    Message = "Room name is required"
                }
            });
        }

        try
        {
            // Получаем userId из токена
            var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var adminId))
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

            var room = await _roomService.CreateRoomAsync(request, adminId);

            return CreatedAtAction(nameof(GetRooms), new { id = room.Id }, new { room });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
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