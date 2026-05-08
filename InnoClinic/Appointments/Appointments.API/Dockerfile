# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082
EXPOSE 8083


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Appointments/Appointments.API/Appointments.API.csproj", "Appointments/Appointments.API/"]
COPY ["Appointments/Appointments.Domain/Appointments.Domain.csproj", "Appointments/Appointments.Domain/"]
COPY ["Appointments/Appointments.Infrastructure/Appointments.Infrastructure.csproj", "Appointments/Appointments.Infrastructure/"]
COPY ["Shared/Shared/Shared.csproj", "Shared/Shared/"]

RUN dotnet restore "./Appointments/Appointments.API/Appointments.API.csproj"
COPY Appointments/ Appointments/
COPY Shared/ Shared/
WORKDIR "/src/Appointments/Appointments.API"
RUN dotnet build "./Appointments.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Appointments.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Appointments.API.dll"]
