---
name: notification-email
description: Add a new notification email end to end — message contract in the shared InnoClinic.Messaging package, enqueueing it via the outbox in Profiles or Appointments, a MassTransit consumer and Scriban HTML template in Notifications.Worker — and the deployment order that avoids lost messages. Use when a new email or notification message is needed, or an existing message contract changes.
---

# Add a notification email

The flow is: a service calls `INotificationProducer.Enqueue(contract)`, which writes an outbox row in the same transaction. Its `NotificationWorker` (`PostgresOutboxProcessor`) publishes the row to RabbitMQ. Notifications.Worker's consumer then renders a template and sends the mail. Use `AppointmentBooked` as the reference: `Shared/InnoClinic.Messaging/Contracts/AppointmentBooked.cs`, `BookAppointmentHandler`, `Consumers/AppointmentBookedConsumer.cs`, `Templates/AppointmentBookedEmail.html`.

## 1. Contract (shared package)

- Add a positional `record` to `Backend/Shared/InnoClinic.Messaging/Contracts/` (namespace `InnoClinic.Messaging.Contracts`). It must carry everything the email needs (recipient email, names, dates), because the worker has no database or gRPC access.
- **Don't rename or move existing contracts.** Outbox rows store the type as `"<FullName>, InnoClinic.Messaging"` and are resolved with `Type.GetType`, so pending rows with the old name would fail to publish. MassTransit also routes by type name. Adding a field is safe; removing or renaming one breaks consumers that are still running the old version.
- Bump `<Version>` in `InnoClinic.Messaging.csproj`. Build the services against it with the `shared-package-local` skill.

## 2. Producer (Profiles or Appointments)

Inject `INotificationProducer` and call `Enqueue(new XContract(...))` **before** `SaveChangesAsync`, so the message commits with the business change. Never publish to MassTransit directly.

## 3. Consumer and template (`Notifications/Notifications.Worker`)

- **Template:** `Templates/<Name>Email.html`, embedded automatically (`Templates\**\*`). Follow the structure of the existing templates (`email-container`, `email-title`, `info-card` with inline styles, because email clients ignore most CSS). Escape every value: `{{ patient_name | html.escape }}`. Scriban exposes model properties in snake_case.
- **Composer** (`Services/EmailComposer.cs`): add a `private static readonly Template XTemplate = ParseTemplate("<Name>Email.html");` and a `Compose<Name>Email(...)` method that returns `new EmailContent(subject, WrapWithStyles(rendered))`. Pre-format dates in C# (`ToString("f")`). The subject ends with ` - InnoClinic`.
- **Consumer** (`Consumers/<Name>Consumer.cs`): `IConsumer<XContract>`. Log with a `[LoggerMessage]` method added to `Extensions/LoggerExtensions.cs`, compose, then `await emailSender.SendAsync(to, content, context.CancellationToken)`. Retries and resilience are handled by `EmailSenderService` and MassTransit, so don't catch exceptions here.
- **Register** it with `x.AddConsumer<XConsumer>();` in `Extensions/MessagingExtensions.cs`.

## 4. Verify

```bash
cd Backend
dotnet build InnoClinic.Notifications.slnx --no-restore
dotnet build InnoClinic.<ProducerService>.slnx --no-restore
```

Notifications has no tests. For an end-to-end check, run docker compose: emails land in Mailpit at http://localhost:8025. Say so if you didn't run it.

## 5. Deployment order (tell the user)

1. Publish the new `InnoClinic.Messaging` version and update `Directory.Packages.props`.
2. Deploy **Notifications.Worker first**. MassTransit creates the exchange-to-queue binding when the consumer starts, and RabbitMQ drops anything published to an exchange with no bound queue. If the producer ships first, those emails are lost, and the outbox still marks them as sent.
3. Then deploy the producing service.
