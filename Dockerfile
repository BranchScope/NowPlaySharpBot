# Base stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app

ENV CONTAINERIZE_THESE_NUTS=true
ENV APP_UID=1001
ENV APP_GID=1001

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NowPlaySharpBot.csproj", "./"]
RUN dotnet restore "NowPlaySharpBot.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "NowPlaySharpBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NowPlaySharpBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
USER root
WORKDIR /app
RUN apt-get update && apt-get install -y curl ffmpeg
RUN echo "Downloading yt-dlp..." && \
    curl -L -o /app/yt-dlp https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux && \
    chmod +x /app/yt-dlp && \
    echo "Download complete. File details:" && \
    ls -l /app/yt-dlp && \
    echo "Listing /app directory contents:" && \
    ls -l /app

COPY --from=publish /app/publish .

# Create and set permissions for the working directory
RUN mkdir -p /app/workdir && \
    mkdir -p /app/workdir/.cache && \
    chown -R ${APP_UID}:${APP_GID} /app/workdir /app/workdir/.cache

USER ${APP_UID}
WORKDIR /app
ENV XDG_CACHE_HOME=/app/workdir/.cache
ENTRYPOINT ["dotnet", "/app/NowPlaySharpBot.dll"]
