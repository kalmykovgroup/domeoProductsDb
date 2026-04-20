FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DomeoProductsDb.Domain/DomeoProductsDb.Domain.csproj",               "src/DomeoProductsDb.Domain/"]
COPY ["src/DomeoProductsDb.Application/DomeoProductsDb.Application.csproj",     "src/DomeoProductsDb.Application/"]
COPY ["src/DomeoProductsDb.Infrastructure/DomeoProductsDb.Infrastructure.csproj","src/DomeoProductsDb.Infrastructure/"]
COPY ["src/DomeoProductsDb.ProductsService/DomeoProductsDb.ProductsService.csproj","src/DomeoProductsDb.ProductsService/"]
RUN dotnet restore "src/DomeoProductsDb.ProductsService/DomeoProductsDb.ProductsService.csproj"
COPY . .
WORKDIR "/src/src/DomeoProductsDb.ProductsService"
RUN dotnet publish "./DomeoProductsDb.ProductsService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DomeoProductsDb.ProductsService.dll"]
