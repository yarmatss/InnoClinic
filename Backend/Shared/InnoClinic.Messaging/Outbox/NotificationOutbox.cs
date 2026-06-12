namespace InnoClinic.Messaging.Outbox;

public class NotificationOutbox
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public OutboxStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
