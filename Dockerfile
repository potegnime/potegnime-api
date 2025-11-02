# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY PotegniMe/*.csproj ./PotegniMe/
RUN dotnet restore ./PotegniMe/PotegniMe.csproj

# Copy the rest of the application code
COPY PotegniMe/ ./PotegniMe/

# Build and publish the application
WORKDIR /app/PotegniMe
RUN dotnet publish -c Release -o /app/out

# Use the official ASP.NET Core runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port your app runs on (default for ASP.NET Core is 8080 in Docker)
EXPOSE 8080

# Set environment variables (optional, adjust as needed)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "PotegniMe.dll"]