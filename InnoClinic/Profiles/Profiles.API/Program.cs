using FluentValidation;
using Profiles.API.Authorization;
using Profiles.API.Endpoints;
using Profiles.API.Extensions;
using Profiles.API.Mapping;
using Profiles.API.Middlewares;
using Profiles.API.Validators;
using Profiles.BLL;
using Profiles.DAL;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

MapsterConfig.Configure();

builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddScopePolicies();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5500")
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

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("Frontend");

app.MapPatientEndpoints();
app.MapMedicalStaffEndpoints();
app.MapSpecializationEndpoints();

app.Run();
