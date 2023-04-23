#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Btms.API/Btms.API.csproj", "Btms.API/"]
RUN dotnet restore "Btms.API/Btms.API.csproj"
COPY . .
WORKDIR "/src/Btms.API"
RUN dotnet build "Btms.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Btms.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Btms.API.dll"]