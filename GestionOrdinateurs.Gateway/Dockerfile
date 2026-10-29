FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj ./GestionOrdinateurs.Gateway/
RUN dotnet restore ./GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj

COPY GestionOrdinateurs.Gateway/ ./GestionOrdinateurs.Gateway/
RUN dotnet publish ./GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5010
ENTRYPOINT ["dotnet", "GestionOrdinateurs.Gateway.dll"]