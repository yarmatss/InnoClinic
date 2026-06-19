namespace Notifications.Worker.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string SenderName { get; set; }
    public required string SenderEmail { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}
