namespace InnoClinic.Messaging.Contracts;

public record PatientCreated(
    Guid PatientId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email);
