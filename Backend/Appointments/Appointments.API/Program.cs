using Appointments.API.BackgroundJobs;
using Appointments.API.Behaviors;
using Appointments.API.Authorization;
using Appointments.API.GrpcHandlers;
using Appointments.API.Options;
using Appointments.Domain.Entities;
using Appointments.Infrastructure;
using FluentValidation;
using InnoClinic.AspNetCore.Authorization;
using InnoClinic.AspNetCore.Cors;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.AspNetCore.Middlewares;
using InnoClinic.Core.Authorization;
using InnoClinic.Messaging.Outbox;
using InnoClinic.Messaging.Extensions;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppObservability("appointments-api");

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOutboxResilience();

builder.Services.Configure<OutboxOptions>(
    builder.Configuration.GetSection(OutboxOptions.SectionName));

builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddClinicAuth(builder.Configuration);
builder.Services.AddSingleton<IAccessPolicy<Appointment>, AppointmentAccessPolicy>();
builder.Services.AddSingleton<IAccessPolicy<PatientRef>, AppointmentAccessPolicy>();
builder.Services.AddSingleton<IAccessPolicy<StaffRef>, AppointmentAccessPolicy>();

builder.Services.AddFrontendCors(builder.Configuration);

builder.Services.Configure<ClinicOptions>(
    builder.Configuration.GetSection(ClinicOptions.SectionName));

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddGrpc();

builder.Services.AddAppHealthChecks(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.MapAppHealthChecks();

app.MapGrpcService<StaffScheduleSyncHandler>()
    .RequireInternalScope(InternalScopes.Appointments);

app.Run();
