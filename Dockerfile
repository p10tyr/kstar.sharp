#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat


FROM microsoft/dotnet:2.1-aspnetcore-runtime as base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production


FROM microsoft/dotnet:2.1-sdk AS builder
ARG Configuration=Release
WORKDIR /src
#COPY *.sln ./
COPY kstar.sharp.aspnetcore/kstar.sharp.aspnetcore.csproj kstar.sharp.aspnetcore/
COPY kstar.sharp/kstar.sharp.csproj kstar.sharp/
COPY kstar.sharp.domain/kstar.sharp.domain.csproj kstar.sharp.domain/
COPY kstar.sharp.ef/kstar.sharp.ef.csproj kstar.sharp.ef/
COPY sqlite/inverter-data.db sqlite/inverter-data.db 
COPY . .

FROM builder AS publish
WORKDIR /src/kstar.sharp.aspnetcore
ARG Configuration=Release
RUN dotnet publish -c $Configuration -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=publish /src/sqlite/inverter-data.db /sqlite/inverter-data.db
ENTRYPOINT ["dotnet", "kstar.sharp.aspnetcore.dll"]


# sudo docker run  -it -p 5001:80 <img>

## debugging build step issues
# sudo docker container run -it --name=debug <buid step id> /bin/sh
# sudo docker rm debug