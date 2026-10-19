using System.ComponentModel.DataAnnotations;

namespace InnoClinic.AspNetCore.Cors;

public sealed class FrontendCorsOptions
{
    public const string SectionName = "Cors";

    [Required]
    [MinLength(1)]
    public required string[] AllowedOrigins { get; set; }
}
