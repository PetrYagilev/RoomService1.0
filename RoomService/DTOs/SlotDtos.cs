namespace RoomService.DTOs;
public class SlotResponse
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class SlotsListResponse
{
    public List<SlotResponse> Slots { get; set; } = new();
}
