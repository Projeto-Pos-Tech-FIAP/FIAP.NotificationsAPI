FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FIAP.NotificationsAPI.Api/FIAP.NotificationsAPI.Api.csproj", "src/FIAP.NotificationsAPI.Api/"]
COPY ["src/FIAP.NotificationsAPI.Application/FIAP.NotificationsAPI.Application.csproj", "src/FIAP.NotificationsAPI.Application/"]
COPY ["src/FIAP.NotificationsAPI.Domain/FIAP.NotificationsAPI.Domain.csproj", "src/FIAP.NotificationsAPI.Domain/"]
COPY ["src/FIAP.NotificationsAPI.Infrastructure/FIAP.NotificationsAPI.Infrastructure.csproj", "src/FIAP.NotificationsAPI.Infrastructure/"]
RUN dotnet restore "src/FIAP.NotificationsAPI.Api/FIAP.NotificationsAPI.Api.csproj"
COPY . .
RUN dotnet publish "src/FIAP.NotificationsAPI.Api/FIAP.NotificationsAPI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FIAP.NotificationsAPI.Api.dll"]
