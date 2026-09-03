using Appointments.Domain.Enums;
using Appointments.Functions.Options;
using Appointments.Infrastructure.Data;
using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Messaging.Contracts;
using InnoClinic.Messaging.Outbox;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Appointments.Functions;

public class AppointmentReminderFunction(
    AppointmentsDbContext dbContext,
    INotificationProducer notificationProducer,
    PatientService.PatientServiceClient patientClient,
    StaffScheduleSyncService.StaffScheduleSyncServiceClient staffClient,
    IOptions<ReminderOptions> options,
    ILogger<AppointmentReminderFunction> logger)
{
    [Function(nameof(AppointmentReminderFunction))]
    public async Task Run(
        [TimerTrigger("%ReminderOptions:Schedule%")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        var config = options.Value;
        var now = DateTime.UtcNow;
        var horizon = now.AddHours(config.LeadTimeHours);

        var appointments = await dbContext.Appointments
            .Where(a => !a.ReminderSent
                     && (a.Status == AppointmentStatus.Planned || a.Status == AppointmentStatus.Confirmed)
                     && a.StartTime <= horizon
                     && a.StartTime > now)
            .Take(config.BatchSize)
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            logger.LogInformation("No upcoming appointments found for reminders between {Now} and {Horizon}.", now, horizon);
            return;
        }

        logger.LogInformation("Found {Count} upcoming appointment(s) to process for reminders.", appointments.Count);

        foreach (var appointment in appointments)
        {
            try
            {
                var patient = await patientClient.GetPatientAsync(
                    new GetPatientRequest { PatientId = appointment.PatientId.ToString() },
                    cancellationToken: cancellationToken);

                if (!patient.Exists)
                {
                    logger.LogWarning("Patient with ID {PatientId} not found for appointment {AppointmentId}. Skipping reminder.", appointment.PatientId, appointment.Id);
                    continue;
                }

                var staff = await staffClient.GetStaffProfileAsync(
                    new GetStaffProfileRequest { MedicalStaffId = appointment.MedicalStaffId.ToString() },
                    cancellationToken: cancellationToken);

                var patientFullName = $"{patient.FirstName} {patient.LastName}".Trim();
                var staffFullName = $"{staff.FirstName} {staff.LastName}".Trim();

                notificationProducer.Enqueue(new AppointmentReminder(
                    appointment.Id,
                    appointment.PatientId,
                    appointment.MedicalStaffId,
                    appointment.StartTime,
                    appointment.EndTime,
                    patient.Email,
                    patientFullName,
                    staffFullName));

                appointment.ReminderSent = true;

                logger.LogInformation("Enqueued reminder notification for appointment {AppointmentId} (Patient: {PatientEmail}).", appointment.Id, patient.Email);
            }
            catch (RpcException rpcEx)
            {
                logger.LogError(rpcEx, "gRPC error while retrieving details for appointment {AppointmentId}. Error: {Status}", appointment.Id, rpcEx.Status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process reminder for appointment {AppointmentId}.", appointment.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
