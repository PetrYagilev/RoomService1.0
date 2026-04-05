using System.ComponentModel.DataAnnotations;

namespace RoomService.Models;

public class Room
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Capacity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Schedule? Schedule { get; set; }
    public ICollection<Slot> Slots { get; set; } = new List<Slot>();
}