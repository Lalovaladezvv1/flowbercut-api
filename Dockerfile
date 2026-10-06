# ============================================
# BUILD
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar primero el proyecto para aprovechar
# la caché de Docker de restore
COPY Flowbercut.Api.csproj ./

RUN dotnet restore "Flowbercut.Api.csproj"

# Copiar el resto del código
COPY . .

# Publicar aplicación
RUN dotnet publish "Flowbercut.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# ============================================
# RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# La API escuchará dentro del contenedor
# en el puerto 8080.
ENV ASPNETCORE_URLS=http://+:8080

# Ambiente por defecto
ENV ASPNETCORE_ENVIRONMENT=Production

# Copiar publicación
COPY --from=build /app/publish .

# Documentación del puerto utilizado
EXPOSE 8080

# Ejecutar API
ENTRYPOINT ["dotnet", "Flowbercut.Api.dll"]