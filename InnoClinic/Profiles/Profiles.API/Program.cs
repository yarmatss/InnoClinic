using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.DataProtection;
using Profiles.API.Authorization;
using Profiles.API.BackgroundJobs;
using Profiles.API.Constants;
using Profiles.API.Endpoints;
using Profiles.API.Extensions;
using Profiles.API.Middlewares;
using Profiles.API.Validators;
using Profiles.BLL;
using Profiles.DAL;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddScopePolicies();

var dataProtectionPath = builder.Configuration["DataProtection:Path"] ?? "/app/keys";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration["Cors:Frontend"] ?? string.Empty;

    options.AddPolicy(CorsPolicies.Frontend, policy =>
        policy.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureFluentValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHostedService<OutboxProcessorJob>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.ApplyMigrationsAsync();
}

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(CorsPolicies.Frontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapPatientEndpoints();
app.MapMedicalStaffEndpoints();
app.MapSpecializationEndpoints();

app.Run();
