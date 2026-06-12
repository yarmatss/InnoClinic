namespace InnoClinic.Messaging.Outbox;

public enum OutboxStatus
{
    Pending,
    Processed,
    Failed
}
