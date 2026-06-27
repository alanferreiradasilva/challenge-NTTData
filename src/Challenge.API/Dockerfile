FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Challenge.API/Challenge.API.csproj Challenge.API/
COPY Challenge.Application/Challenge.Application.csproj Challenge.Application/
COPY Challenge.Domain/Challenge.Domain.csproj Challenge.Domain/
COPY Challenge.Infrastructure/Challenge.Infrastructure.csproj Challenge.Infrastructure/
RUN dotnet restore Challenge.API/Challenge.API.csproj

COPY . .
WORKDIR /src/Challenge.API
RUN dotnet publish Challenge.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

VOLUME /app/data

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Challenge.API.dll"]
