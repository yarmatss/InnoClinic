using Appointments.API.Behaviors;
using Appointments.API.GrpcHandlers;
using Appointments.Infrastructure;
using FluentValidation;
using InnoClinic.AspNetCore.Extensions;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpLogging(o => { });

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddGrpc();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.ApplyMigrationsAsync();
}

app.UseHttpLogging();

app.MapEndpoints();

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.MapGrpcService<StaffScheduleSyncHandler>();

app.Run();
