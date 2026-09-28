namespace InnoClinic.Messaging.Contracts;

public record StaffCreated(
    Guid StaffId,
    string FirstName,
    string LastName,
    string Email,
    string? InvitationUrl = null);
