namespace Profiles.BLL.Options;

public class Auth0ManagementOptions
{
    public const string SectionName = "Auth0:Management";

    public string? Domain { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Connection { get; set; } = "Username-Password-Authentication";
    public string? PatientRoleId { get; set; }
    public string? DoctorRoleId { get; set; }
    public string? AdministratorRoleId { get; set; }
    public string? ReceptionistRoleId { get; set; }
    public string? InvitationRedirectUrl { get; set; } = "http://localhost:5500";
    public int InvitationTicketTtlSec { get; set; } = 432000;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Domain) &&
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret);
}
