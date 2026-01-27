FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8500
EXPOSE 8501

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TroLiKOC.API/TroLiKOC.API.csproj", "src/TroLiKOC.API/"]
COPY ["src/TroLiKOC.SharedKernel/TroLiKOC.SharedKernel.csproj", "src/TroLiKOC.SharedKernel/"]
COPY ["src/TroLiKOC.JobOrchestrator/TroLiKOC.JobOrchestrator.csproj", "src/TroLiKOC.JobOrchestrator/"]
COPY ["src/Modules/TroLiKOC.Modules.Identity/TroLiKOC.Modules.Identity.csproj", "src/Modules/TroLiKOC.Modules.Identity/"]
COPY ["src/Modules/TroLiKOC.Modules.Wallet/TroLiKOC.Modules.Wallet.csproj", "src/Modules/TroLiKOC.Modules.Wallet/"]
COPY ["src/Modules/TroLiKOC.Modules.Subscription/TroLiKOC.Modules.Subscription.csproj", "src/Modules/TroLiKOC.Modules.Subscription/"]
COPY ["src/Modules/TroLiKOC.Modules.Jobs/TroLiKOC.Modules.Jobs.csproj", "src/Modules/TroLiKOC.Modules.Jobs/"]
COPY ["src/Modules/TroLiKOC.Modules.Payment/TroLiKOC.Modules.Payment.csproj", "src/Modules/TroLiKOC.Modules.Payment/"]
RUN dotnet restore "./src/TroLiKOC.API/TroLiKOC.API.csproj"
COPY . .
WORKDIR "/src/src/TroLiKOC.API"
RUN dotnet build "./TroLiKOC.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TroLiKOC.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TroLiKOC.API.dll"]
