# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj files and restore
COPY ["AutoReparos.API/AutoReparos.API.csproj", "AutoReparos.API/"]
COPY ["AutoReparos.Application/AutoReparos.Application.csproj", "AutoReparos.Application/"]
COPY ["AutoReparos.Domain/AutoReparos.Domain.csproj", "AutoReparos.Domain/"]
COPY ["AutoReparos.Infra/AutoReparos.Infra.csproj", "AutoReparos.Infra/"]

RUN dotnet restore "AutoReparos.API/AutoReparos.API.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/AutoReparos.API"
RUN dotnet build "AutoReparos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Generate migration bundle
RUN dotnet ef migrations bundle --project ../AutoReparos.Infra/AutoReparos.Infra.csproj --startup-project AutoReparos.API.csproj --output /app/build/migrate --configuration $BUILD_CONFIGURATION --self-contained -r linux-x64

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AutoReparos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN cp /app/build/migrate /app/publish/migrate

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN chmod +x migrate
USER $APP_UID

ENTRYPOINT ["sh", "-c", "./migrate && dotnet AutoReparos.API.dll"]
