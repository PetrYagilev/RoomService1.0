using System.ComponentModel.DataAnnotations;

namespace RoomService.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; } // Nullable for dummy users

    [Required]
    public string Role { get; set; } = "user"; // admin or user

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}