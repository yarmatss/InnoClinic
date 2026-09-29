using System.Reflection;
using InnoClinic.Messaging.Contracts;
using Microsoft.Extensions.Options;
using Notifications.Worker.Models;
using Notifications.Worker.Options;
using Scriban;

namespace Notifications.Worker.Services;

public class EmailComposer(IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    private const string TemplatesPrefix = "Notifications.Worker.Templates";

    private static readonly string SharedCss = LoadEmbeddedResource("email.css");

    private static readonly Template AccountEmailTemplate = ParseTemplate("AccountCreatedEmail.html");

    private static readonly Template AppointmentBookedTemplate = ParseTemplate("AppointmentBookedEmail.html");

    private static readonly Template AppointmentReminderTemplate = ParseTemplate("AppointmentReminderEmail.html");

    public EmailContent ComposeAccountEmail(
        AccountCreated message,
        string subject,
        string welcomeText,
        string passwordLinkText)
    {
        var invitationUrl = string.IsNullOrWhiteSpace(message.InvitationUrl) ? null : message.InvitationUrl;

        var model = new
        {
            FirstName = message.FirstName,
            LastName = message.LastName,
            WelcomeText = welcomeText,
            PasswordLinkText = passwordLinkText,
            InvitationUrl = invitationUrl,
            PortalUrl = _frontend.BaseUrl
        };

        var renderedHtml = AccountEmailTemplate.Render(model);
        return new EmailContent(subject, WrapWithStyles(renderedHtml));
    }

    public static EmailContent ComposeAppointmentBookedEmail(
        string patientName,
        string medicalStaffName,
        DateTime startTime)
    {
        const string subject = "Appointment Confirmation - InnoClinic";

        var model = new
        {
            PatientName = patientName,
            MedicalStaffName = medicalStaffName,
            FormattedDate = startTime.ToString("f")
        };

        var renderedHtml = AppointmentBookedTemplate.Render(model);
        return new EmailContent(subject, WrapWithStyles(renderedHtml));
    }

    public static EmailContent ComposeAppointmentReminderEmail(
        string patientName,
        string medicalStaffName,
        DateTime startTime)
    {
        const string subject = "Reminder: Upcoming Appointment - InnoClinic";

        var model = new
        {
            PatientName = patientName,
            MedicalStaffName = medicalStaffName,
            FormattedDate = startTime.ToString("f")
        };

        var renderedHtml = AppointmentReminderTemplate.Render(model);
        return new EmailContent(subject, WrapWithStyles(renderedHtml));
    }

    private static string WrapWithStyles(string html) =>
        $"<style>\n{SharedCss}\n</style>\n{html}";

    private static string LoadEmbeddedResource(string fileName)
    {
        var manifestResourceName = $"{TemplatesPrefix}.{fileName}";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(manifestResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{manifestResourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static Template ParseTemplate(string fileName)
    {
        var content = LoadEmbeddedResource(fileName);
        var template = Template.Parse(content);
        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Failed to parse template '{fileName}': {errors}");
        }

        return template;
    }
}
