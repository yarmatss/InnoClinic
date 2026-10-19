using System.ComponentModel.DataAnnotations;

namespace InnoClinic.AspNetCore.Authorization;

public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    [Required]
    public required string Domain { get; set; }

    [Required]
    public required string Audience { get; set; }

    [Required]
    public required string InternalAudience { get; set; }
}
