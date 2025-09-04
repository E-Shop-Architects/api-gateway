# Temel çalışma ortamı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["ApiGateway.csproj", "./"]
RUN dotnet restore "./ApiGateway.csproj"

# Tüm dosyaları kopyala ve publish et
COPY . .
RUN dotnet publish "./ApiGateway.csproj" -c Release -o /app/publish

# Yayınlanan dosyaları çalıştırılacak container'a al
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
