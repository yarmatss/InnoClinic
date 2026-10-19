using InnoClinic.Core.Authorization;

namespace Profiles.BLL.Authorization;

internal sealed class PatientAccessPolicy : IAccessPolicy<PatientRef>, IAccessPolicy<PatientDirectory>
{
    public AccessLevel Evaluate(ICurrentUser user, PatientRef patient, Operation operation) => user.Role switch
    {
        UserRole.Administrator or UserRole.Receptionist => AccessLevel.Full,
        UserRole.Doctor when operation == Operation.Read => AccessLevel.Masked,
        UserRole.Patient when operation == Operation.Read && user.Owns(patient.PatientId) => AccessLevel.Full,
        _ => AccessLevel.None
    };

    public AccessLevel Evaluate(ICurrentUser user, PatientDirectory directory, Operation operation) => user.Role switch
    {
        UserRole.Administrator or UserRole.Receptionist => AccessLevel.Full,
        UserRole.Doctor when operation == Operation.Read => AccessLevel.Masked,
        _ => AccessLevel.None
    };
}
