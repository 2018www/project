# https://hub.docker.com/_/microsoft-dotnet 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution file and project file
COPY BlogA.sln .
COPY BlogA/BlogA.csproj ./BlogA/
RUN dotnet restore 

# Restore dependencies and build the app
COPY BlogA/. ./BlogA/
RUN dotnet publish BlogA/BlogA.csproj -c release -o /app --no-restore


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "BlogA.dll"]