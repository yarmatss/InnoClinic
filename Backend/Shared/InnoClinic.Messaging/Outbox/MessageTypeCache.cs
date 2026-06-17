namespace InnoClinic.Messaging.Outbox;

public static class MessageTypeCache<T>
{
    public static readonly string Name = $"{typeof(T).FullName}, {typeof(T).Assembly.GetName().Name}";
}
