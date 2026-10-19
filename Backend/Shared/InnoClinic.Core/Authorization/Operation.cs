namespace InnoClinic.Core.Authorization;

public readonly record struct Operation(string Name)
{
    public static readonly Operation Read = new(nameof(Read));
    public static readonly Operation Create = new(nameof(Create));
    public static readonly Operation Update = new(nameof(Update));
    public static readonly Operation Delete = new(nameof(Delete));

    public override string ToString() => Name;
}
