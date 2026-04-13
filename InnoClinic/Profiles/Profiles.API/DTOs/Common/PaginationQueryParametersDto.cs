namespace Profiles.API.DTOs.Common;

public abstract class PaginationQueryParametersDto
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
}
