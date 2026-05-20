namespace Appointments.Domain.Exceptions;

public class ScheduleConflictException(string message) : Exception(message);
