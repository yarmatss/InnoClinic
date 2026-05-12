namespace Profiles.API.Options;

public class OutboxOptions
{
    public int IntervalInSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
}
