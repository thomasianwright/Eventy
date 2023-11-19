FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["demo/Event.Demo.Api/Event.Demo.Api.csproj", "demo/Event.Demo.Api/"]
COPY ["source/transports/Eventy.Transports.RabbitMQ/Eventy.Transports.RabbitMQ.csproj", "source/transports/Eventy.Transports.RabbitMQ/"]
COPY ["source/core/Eventy.Abstractions/Eventy.Abstractions.csproj", "source/core/Eventy.Abstractions/"]
COPY ["source/core/Eventy.Core/Eventy.Core.csproj", "source/core/Eventy.Core/"]
RUN dotnet restore "demo/Event.Demo.Api/Event.Demo.Api.csproj"
COPY . .
WORKDIR "/src/demo/Event.Demo.Api"
RUN dotnet build "Event.Demo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Event.Demo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Event.Demo.Api.dll"]
