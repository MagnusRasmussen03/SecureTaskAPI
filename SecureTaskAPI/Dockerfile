# Byg fasen
# Hent et Linux-miljø med .NET SDK installeret — vi kalder det "build"
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Gå ind i mappen /app (opretter den hvis den ikke findes)
WORKDIR /app

# Kopier projektfilen ind og download alle pakker (Npgsql osv.)
# Kopier projektfilen ind og download alle pakker (Npgsql osv.)
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Kør fasen
# Hent et nyt, mindre Linux-miljø — kun til at KØRE .NET, ikke bygge
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Kopier kun det færdigbyggede resultat fra fase 1
COPY --from=build /app/publish .

# Når containeren starter, kør dette kommando
ENTRYPOINT ["dotnet", "SecureTaskAPI.dll"]