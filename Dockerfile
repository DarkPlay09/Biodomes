FROM registry.helmo.be/microsoft/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM registry.helmo.be/microsoft/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BioDomes.Web/BioDomes.Web.csproj", "BioDomes.Web/"]
RUN dotnet restore "BioDomes.Web/BioDomes.Web.csproj"
COPY . .
WORKDIR "/src/BioDomes.Web"
RUN dotnet build "BioDomes.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BioDomes.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BioDomes.Web.dll"]
