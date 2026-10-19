using Appointments.Domain.Entities;
using InnoClinic.Core.Authorization;

namespace Appointments.API.Authorization;

internal sealed class AppointmentAccessPolicy
    : IAccessPolicy<Appointment>, IAccessPolicy<PatientRef>, IAccessPolicy<StaffRef>
{
    public AccessLevel Evaluate(ICurrentUser user, Appointment appointment, Operation operation)
    {
        if (user.IsInRole(UserRole.Administrator))
            return AccessLevel.Full;

        if (operation == AppointmentOperations.Confirm || operation == AppointmentOperations.SubmitResult)
            return Allow(user.IsInRole(UserRole.Doctor) && user.Owns(appointment.MedicalStaffId));

        if (operation == Operation.Read || operation == AppointmentOperations.Cancel)
        {
            return user.Role switch
            {
                UserRole.Receptionist => AccessLevel.Full,
                UserRole.Patient => Allow(user.Owns(appointment.PatientId)),
                UserRole.Doctor => Allow(user.Owns(appointment.MedicalStaffId)),
                _ => AccessLevel.None
            };
        }

        return AccessLevel.None;
    }

    public AccessLevel Evaluate(ICurrentUser user, PatientRef patient, Operation operation)
    {
        if (operation != Operation.Read && operation != AppointmentOperations.Book)
            return AccessLevel.None;

        return user.Role switch
        {
            UserRole.Administrator or UserRole.Receptionist => AccessLevel.Full,
            UserRole.Patient => Allow(user.Owns(patient.PatientId)),
            _ => AccessLevel.None
        };
    }

    public AccessLevel Evaluate(ICurrentUser user, StaffRef staff, Operation operation)
    {
        if (operation != Operation.Read)
            return AccessLevel.None;

        return user.Role switch
        {
            UserRole.Administrator or UserRole.Receptionist => AccessLevel.Full,
            UserRole.Doctor => Allow(user.Owns(staff.StaffId)),
            _ => AccessLevel.None
        };
    }

    private static AccessLevel Allow(bool condition) => condition ? AccessLevel.Full : AccessLevel.None;
}
