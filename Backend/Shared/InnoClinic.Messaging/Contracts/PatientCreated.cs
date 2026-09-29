namespace InnoClinic.Messaging.Contracts;

public record PatientCreated(
    Guid PatientId,
    string FirstName,
    string LastName,
    string Email,
    string? InvitationUrl = null)
    : AccountCreated(FirstName, LastName, Email, InvitationUrl);
