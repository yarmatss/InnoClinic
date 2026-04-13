namespace Profiles.BLL.Models;

public abstract class BaseQueryModel
{
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
