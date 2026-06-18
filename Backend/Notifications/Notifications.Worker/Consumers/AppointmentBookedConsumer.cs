using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class AppointmentBookedConsumer(
    IEmailSenderService emailSender,
    ILogger<AppointmentBookedConsumer> logger) 
    : IConsumer<AppointmentBooked>
{
    public async Task Consume(ConsumeContext<AppointmentBooked> context)
    {
        var message = context.Message;

        logger.LogAppointmentBookingNotificationProcessing(message.AppointmentId, message.PatientId);

        var (subject, body) = BuildAppointmentEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, subject, body, context.CancellationToken);
    }

    private static (string Subject, string Body) BuildAppointmentEmail(
        string patientName,
        string medicalStaffName,
        DateTime startTime)
    {
        var subject = "Appointment Confirmation - InnoClinic";

        var formattedDate = startTime.ToString("f");

        var body = $"""
            <h1>Appointment Confirmed!</h1>
            <p>Dear {patientName},</p>
            <p>Your appointment has been successfully scheduled and approved.</p>
            <hr />
            <ul>
                <li><strong>Doctor / Specialist:</strong> {medicalStaffName}</li>
                <li><strong>Date & Time:</strong> {formattedDate}</li>
            </ul>
            """;

        return (subject, body);
    }
}
