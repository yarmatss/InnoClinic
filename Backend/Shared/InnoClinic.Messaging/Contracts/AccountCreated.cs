namespace InnoClinic.Messaging.Contracts;

public abstract record AccountCreated(
    string FirstName,
    string LastName,
    string Email,
    string? InvitationUrl = null);
