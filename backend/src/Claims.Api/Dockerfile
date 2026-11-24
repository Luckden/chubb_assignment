# Build context: ./backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ClaimsPlatform.slnx ./
COPY src/Claims.Api/Claims.Api.csproj src/Claims.Api/
RUN dotnet restore src/Claims.Api/Claims.Api.csproj
COPY src/ src/
RUN dotnet publish src/Claims.Api/Claims.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Claims.Api.dll"]
