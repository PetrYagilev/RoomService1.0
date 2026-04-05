using RoomService.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomService.Models;


public class Slot
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [Required]
    public DateTime Start { get; set; } // UTC

    [Required]
    public DateTime End { get; set; } // UTC

    // Navigation properties
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    public Booking? Booking { get; set; }
}