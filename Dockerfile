FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app

# Copy everthing
COPY . .
RUN dotnet restore 
RUN dotnet publish -o /app/published-app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Respect the port your app binds to
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/published-app /app
ENTRYPOINT [ "dotnet", "/app/Authentication.dll" ]