#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
RUN mkdir /sqlite
VOLUME /sqlite
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
ARG Configuration=Release
WORKDIR /src
COPY kstar.sharp.console/kstar.sharp.console.csproj kstar.sharp.console/
COPY kstar.sharp/kstar.sharp.csproj kstar.sharp/
COPY kstar.sharp.domain/kstar.sharp.domain.csproj kstar.sharp.domain/
COPY kstar.sharp.ef/kstar.sharp.ef.csproj kstar.sharp.ef/
COPY kstar.sharp.datacollect/kstar.sharp.datacollect.csproj kstar.sharp.datacollect/
#COPY sqlite/inverter-data.db sqlite/inverter-data.db 
COPY . .

FROM builder AS publish
WORKDIR /src/kstar.sharp.console
ARG Configuration=Release
RUN dotnet publish -c $Configuration -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
#COPY --from=publish /src/sqlite/inverter-data.db /sqlite/inverter-data.db
ENTRYPOINT ["dotnet", "kstar.sharp.console.dll"]


# Change {host path} to where your sqlite file is 
# sudo docker run --volume={host path}:/sqlite -it kstar.sharp/console -- --ip-192.168.1.50 --sqlite-"Data Source=/sqlite/inverter-data.db"

# getting bash while container running
# docker exec -it <container name> /bin/bash

## debugging build step issues
# sudo docker container run -it --name=debug <buid step id> /bin/sh
# sudo docker rm debug