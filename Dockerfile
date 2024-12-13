FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app

# Copy everthing
COPY . .
RUN dotnet restore 
RUN dotnet publish -o /app/published-app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
EXPOSE 8080

WORKDIR /app
COPY --from=build /app/published-app /app
ENTRYPOINT [ "dotnet", "/app/Authentication.dll" ]