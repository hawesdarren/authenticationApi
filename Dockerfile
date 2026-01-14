FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src

# Copy everthing
COPY . .
RUN dotnet restore 
RUN dotnet publish -o /app/published-app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim as runtime
WORKDIR /app

# Install curl for healthchecks and CA bundle
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy certs from from build context (authenticationApi/certs)
COPY certs/ /tmp/certs/
RUN if [ -d /tmp/certs ] && ls /tmp/certs/*.crt >/dev/null 2>&1; then \
      cp /tmp/certs/*.crt /usr/local/share/ca-certificates/; \
      update-ca-certificates; \
    else \
      echo "No custom CA certs found; skipping."; \
    fi

# Create parent dir for the mount
RUN mkdir -p /app/secret-volume

# Respect the port your app binds to
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/published-app /app
ENTRYPOINT [ "dotnet", "/app/Authentication.dll" ]