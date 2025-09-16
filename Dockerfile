# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV SOAP_ENDPOINT=https://run.mocky.io/v3/19217075-6d4e-4818-98bc-416d1feb7b84
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "acme-envio-pedidos.dll"]
