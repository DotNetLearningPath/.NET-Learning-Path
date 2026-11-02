FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["src/SupportAPI.Api/SupportAPI.Api.csproj", "SupportAPI.Api/"]
COPY ["src/SupportAPI.Application/SupportAPI.Application.csproj", "SupportAPI.Application/"]
COPY ["src/SupportAPI.Domain/SupportAPI.Domain.csproj", "SupportAPI.Domain/"]
COPY ["src/SupportAPI.Infrastructure/SupportAPI.Infrastructure.csproj", "SupportAPI.Infrastructure/"]

RUN dotnet restore "SupportAPI.Api/SupportAPI.Api.csproj"

COPY src/ .
WORKDIR /source/SupportAPI.Api
RUN dotnet publish "SupportAPI.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app

COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080

ENTRYPOINT [ "dotnet", "SupportAPI.Api.dll" ]