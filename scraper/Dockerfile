FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY scrapper.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "scraper.dll"]