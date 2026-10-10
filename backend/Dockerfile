FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/UserDirectory.Domain/UserDirectory.Domain.csproj src/UserDirectory.Domain/
COPY src/UserDirectory.Application/UserDirectory.Application.csproj src/UserDirectory.Application/
COPY src/UserDirectory.Infrastructure/UserDirectory.Infrastructure.csproj src/UserDirectory.Infrastructure/
COPY src/UserDirectory.Api/UserDirectory.Api.csproj src/UserDirectory.Api/
RUN dotnet restore src/UserDirectory.Api/UserDirectory.Api.csproj

COPY src/ src/
RUN dotnet publish src/UserDirectory.Api/UserDirectory.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN mkdir -p /data && chown -R app:app /data
USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Database__Path=/data/app.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "UserDirectory.Api.dll"]
