FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ServerWatch.sln && dotnet publish ServerWatch.Web/ServerWatch.Web.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
RUN mkdir -p /data
ENV ASPNETCORE_URLS=http://+:8080 DatabaseProvider=Sqlite ConnectionStrings__ServerWatch="Data Source=/data/serverwatch.db"
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet","ServerWatch.Web.dll"]
