using InnoClinic.Messaging.Constants;

namespace InnoClinic.Messaging.Outbox;

public class OutboxOptions
{
    public const string SectionName = "NotificationOutbox";

    public int BatchSize { get; set; } = 50;
    public int IntervalInSeconds { get; set; } = 10;
    public int MaxRetryCount { get; set; } = 5;
    public string TableName { get; set; } = MessagingConstants.DefaultOutboxTableName;
}
