namespace Profiles.Domain.Models;

public class PaginationParameters
{
    public int? PageNumber
    {
        get => field ?? 1;
        set => field = value;
    }

    public int? PageSize
    {
        get => field ?? 10;
        set => field = value;
    }

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; }

    public bool IsDescending => SortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
}
