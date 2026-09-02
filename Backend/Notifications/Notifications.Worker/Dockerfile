# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["Notifications/Notifications.Worker/Notifications.Worker.csproj", "Notifications/Notifications.Worker/"]

RUN --mount=type=secret,id=NUGET_TOKEN,env=NUGET_TOKEN \
    --mount=type=secret,id=NUGET_USER,env=NUGET_USER \
    dotnet restore "./Notifications/Notifications.Worker/Notifications.Worker.csproj"
COPY Notifications/Notifications.Worker/ Notifications/Notifications.Worker/
WORKDIR "/src/Notifications/Notifications.Worker"
RUN dotnet build "./Notifications.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notifications.Worker.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Worker.dll"]
