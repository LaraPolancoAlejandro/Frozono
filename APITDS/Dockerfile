# Usa la imagen oficial de Microsoft para .NET 6
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usa el SDK de .NET 6 para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["APITDS.csproj", "./"]
RUN dotnet restore "./APITDS.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "APITDS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "APITDS.csproj" -c Release -o /app/publish

# Copia los archivos construidos y publicados a la imagen base
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APITDS.dll"]
