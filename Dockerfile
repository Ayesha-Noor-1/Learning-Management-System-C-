# syntax=docker/dockerfile:1
# Build from repository root (folder that contains LMS-C#/).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/LMS-C#
COPY LMS-C#/LMS-C#.csproj .
RUN dotnet restore "LMS-C#.csproj"

COPY LMS-C#/ ./
RUN dotnet publish "LMS-C#.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV TERM=xterm-256color

ENTRYPOINT ["dotnet", "LMS-C#.dll"]
