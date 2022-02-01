#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["kstar.sharp.workerservice/kstar.sharp.workerservice.csproj", "kstar.sharp.workerservice/"]
RUN dotnet restore "kstar.sharp.workerservice/kstar.sharp.workerservice.csproj"
COPY . .
WORKDIR "/src/kstar.sharp.workerservice"
RUN dotnet build "kstar.sharp.workerservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "kstar.sharp.workerservice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "kstar.sharp.workerservice.dll"]