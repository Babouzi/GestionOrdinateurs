FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copie des projets et restauration
COPY AuthentificationApi/AuthentificationApi.csproj ./AuthentificationApi/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./AuthentificationApi/AuthentificationApi.csproj
RUN dotnet restore ./GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj

# Copie du reste du code et publication
COPY AuthentificationApi/ ./AuthentificationApi/
COPY GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./AuthentificationApi/AuthentificationApi.csproj -c Release -o out

# Image d'exécution
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5157
ENTRYPOINT ["dotnet", "AuthentificationApi.dll"]