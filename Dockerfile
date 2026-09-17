FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GoldenCrown/GoldenCrown.csproj GoldenCrown/
RUN dotnet restore GoldenCrown/GoldenCrown.csproj

COPY GoldenCrown/ GoldenCrown/
RUN dotnet publish GoldenCrown/GoldenCrown.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GoldenCrown.dll"]
