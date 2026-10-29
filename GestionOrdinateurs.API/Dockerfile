FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY GestionOrdinateurs.API/GestionOrdinateurs.API.csproj ./GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./GestionOrdinateurs.API/GestionOrdinateurs.API.csproj
RUN dotnet restore ./GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj

COPY GestionOrdinateurs.API/ ./GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./GestionOrdinateurs.API/GestionOrdinateurs.API.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5197 
ENTRYPOINT ["dotnet", "GestionOrdinateurs.API.dll"]