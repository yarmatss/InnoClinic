using FluentValidation;
using Profiles.API.Endpoints;
using Profiles.API.Mapping;
using Profiles.API.Middlewares;
using Profiles.API.Validators;
using Profiles.BLL;
using Profiles.DAL;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

MapsterConfig.Configure();

builder.Services.AddBusinessLogicLayer(builder.Configuration);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureFluentValidation();

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

public partial class Program { }
