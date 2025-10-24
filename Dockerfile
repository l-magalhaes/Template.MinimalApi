FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj para cache
COPY Template.MinimalApi.Domain/*.csproj Template.MinimalApi.Domain/
COPY Template.MinimalApi.Application/*.csproj Template.MinimalApi.Application/
COPY Template.MinimalApi.Infrastructure/*.csproj Template.MinimalApi.Infrastructure/
COPY Template.MinimalApi.API/*.csproj Template.MinimalApi.API/
COPY Template.MinimalApi.Tests/*.csproj Template.MinimalApi.Tests/

RUN dotnet restore Template.MinimalApi.API/Template.MinimalApi.API.csproj

COPY . .

RUN dotnet publish Template.MinimalApi.API/Template.MinimalApi.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 63180

ENTRYPOINT ["dotnet", "Template.MinimalApi.API.dll"]
