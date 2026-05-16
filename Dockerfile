FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["src/ETD.ServiceDefaults/ETD.ServiceDefaults.csproj", "src/ETD.ServiceDefaults/"]
COPY ["src/ETD.Web/ETD.Web.csproj", "src/ETD.Web/"]
RUN dotnet restore "src/ETD.Web/ETD.Web.csproj"
COPY src/ src/
WORKDIR /src/src/ETD.Web
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "ETD.Web.dll"]
