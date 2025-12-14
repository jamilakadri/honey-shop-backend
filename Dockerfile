FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MielShop.API.csproj", "./"]
RUN dotnet restore "MielShop.API.csproj"
COPY . .
RUN dotnet build "MielShop.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MielShop.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MielShop.API.dll"]