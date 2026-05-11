using Appointments.API.GrpcHandlers;
using Appointments.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGrpc();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.MapGrpcService<StaffScheduleSyncHandler>();

app.Run();
