FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Breezy.Benchmarks/Breezy.Benchmarks.csproj", "Breezy.Benchmarks/"]
RUN dotnet restore "Breezy.Benchmarks/Breezy.Benchmarks.csproj"
COPY . .
WORKDIR "/src/Breezy.Benchmarks"
RUN dotnet build "Breezy.Benchmarks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Breezy.Benchmarks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Breezy.Benchmarks.dll"]
