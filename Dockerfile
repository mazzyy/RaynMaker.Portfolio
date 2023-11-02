
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
COPY . .
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /app

COPY ./build ./build
COPY ./src ./src
COPY ./docs ./docs

RUN dotnet build -c Release -o /app/build /app/src/RaynMaker.Portfolio.Service/RaynMaker.Portfolio.Service.fsproj

FROM build AS publish
RUN dotnet publish "/app/src/RaynMaker.Portfolio.Service/RaynMaker.Portfolio.Service.fsproj"  -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY --from=publish /app/publish .
RUN cp -r /app/docs /docs

EXPOSE  2525 
ENTRYPOINT ["dotnet", "RaynMaker.Portfolio.Service.dll"]

