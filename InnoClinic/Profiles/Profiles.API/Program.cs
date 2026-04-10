using FluentValidation;
using Profiles.API.Endpoints;
using Profiles.API.Middlewares;
using Profiles.BLL;
using Profiles.DAL;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBusinessLogicLayer(builder.Configuration);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
    member != null
        ? JsonNamingPolicy.CamelCase.ConvertName(member.Name)
        : null;

ValidatorOptions.Global.DisplayNameResolver = ValidatorOptions.Global.PropertyNameResolver;

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails();
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

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

app.UseAuthorization();

app.MapPatientEndpoints();
app.MapMedicalStaffEndpoints();
app.MapSpecializationEndpoints();

app.Run();
