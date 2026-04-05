using RoomService.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomService.Models;
public class Schedule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public List<int> DaysOfWeek { get; set; } = new(); // 1=Monday ... 7=Sunday

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }
}
