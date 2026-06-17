namespace InnoClinic.Messaging.Outbox;

public interface INotificationProducer
{
    void Enqueue<T>(T payload) where T : class;
}
