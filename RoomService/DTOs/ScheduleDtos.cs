using System.ComponentModel.DataAnnotations;


namespace RoomService.DTOs;
public class CreateScheduleRequest
{
    [Required]
    public List<int> DaysOfWeek { get; set; } = new(); // 1-7, где 1=Пн, 7=Вс

    [Required]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format (HH:MM)")]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format (HH:MM)")]
    public string EndTime { get; set; } = string.Empty;
}

public class ScheduleResponse
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public List<int> DaysOfWeek { get; set; } = new();
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}