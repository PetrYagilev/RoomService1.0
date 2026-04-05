namespace RoomService.Services;
public class ConferenceLinkService : IConferenceLinkService
{
    private readonly ILogger<ConferenceLinkService> _logger;
    private static readonly Random _random = new();

    public ConferenceLinkService(ILogger<ConferenceLinkService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateConferenceLinkAsync(Guid bookingId)
    {
        // Симулируем задержку внешнего сервиса
        await Task.Delay(100);

        // Симулируем случайные ошибки (10% неудач для тестирования)
        if (_random.Next(100) < 10)
        {
            _logger.LogWarning("Conference service simulation: random failure for booking {BookingId}", bookingId);
            throw new Exception("Conference service temporarily unavailable");
        }

        // Генерируем тестовую ссылку
        var conferenceId = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 12);

        return $"https://meet.example.com/room/{conferenceId}";
    }
}
