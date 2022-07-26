FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY "RedMetrixWebApp.csproj" .
RUN dotnet restore "RedMetrixWebApp.csproj"
COPY . .
RUN dotnet build "RedMetrixWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedMetrixWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedMetrixWebApp.dll"]