using Appointments.API.BackgroundJobs;
using Appointments.API.Behaviors;
using Appointments.API.Authorization;
using Appointments.API.Extensions;
using Appointments.API.GrpcHandlers;
using Appointments.API.Options;
using Appointments.Infrastructure;
using FluentValidation;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.AspNetCore.Middlewares;
using InnoClinic.Messaging.Outbox;
using InnoClinic.Messaging.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxResilience();

builder.Services.Configure<OutboxOptions>(
    builder.Configuration.GetSection(OutboxOptions.SectionName));

builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddScopePolicies();

builder.Services.Configure<ClinicOptions>(
    builder.Configuration.GetSection(ClinicOptions.SectionName));

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod |
                      HttpLoggingFields.RequestPath |
                      HttpLoggingFields.ResponseStatusCode |
                      HttpLoggingFields.Duration;
});

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddGrpc();

builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.ApplyMigrationsAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.MapGrpcService<StaffScheduleSyncHandler>();

app.Run();
