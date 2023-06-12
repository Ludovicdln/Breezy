FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["samples/Breezy.Sample/Breezy.Sample.csproj", "Breezy.Sample/"]
RUN dotnet restore "samples/Breezy.Sample/Breezy.Sample.csproj"
COPY . .
WORKDIR "/src/Breezy.Sample"
RUN dotnet build "Breezy.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Breezy.Sample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Breezy.Sample.dll"]
