using InnoClinic.Core.Authorization;

namespace Appointments.API.Authorization;

public static class AppointmentOperations
{
    public static readonly Operation Book = new(nameof(Book));
    public static readonly Operation Cancel = new(nameof(Cancel));
    public static readonly Operation Confirm = new(nameof(Confirm));
    public static readonly Operation SubmitResult = new(nameof(SubmitResult));
}
