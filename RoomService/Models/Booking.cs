using RoomService.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomService.Models;
public class Booking
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SlotId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Status { get; set; } = "active"; // active, cancelled

    public string? ConferenceLink { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SlotId))]
    public Slot? Slot { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}

