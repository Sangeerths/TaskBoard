# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskBoard/TaskBoard.csproj TaskBoard/
RUN dotnet restore TaskBoard/TaskBoard.csproj

COPY . .
WORKDIR /src/TaskBoard
RUN dotnet publish TaskBoard.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskBoard.dll"]