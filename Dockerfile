# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy project files and restore dependencies
COPY ["ContentAggregator.API/ContentAggregator.API.csproj", "ContentAggregator.API/"]
COPY ["ContentAggregator.Core/ContentAggregator.Core.csproj", "ContentAggregator.Core/"]
COPY ["ContentAggregator.Infrastructure/ContentAggregator.Infrastructure.csproj", "ContentAggregator.Infrastructure/"]
RUN dotnet restore "ContentAggregator.API/ContentAggregator.API.csproj"
# Copy all source code and build
COPY . .
WORKDIR "/src/ContentAggregator.API"
RUN dotnet build "ContentAggregator.API.csproj" -c Release -o /app/build

#FROM build AS publish
RUN dotnet publish "ContentAggregator.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install ffmpeg - required for yt-dlp subtitle formatting
RUN apt-get update && apt-get install -y ffmpeg python3

# Copy published files from build stage
COPY --from=build /app/publish .

# Configure environment and expose port
ENV ASPNETCORE_ENVIRONMENTS=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443
EXPOSE 80
EXPOSE 443

# Define entry point
ENTRYPOINT ["dotnet", "ContentAggregator.API.dll"]