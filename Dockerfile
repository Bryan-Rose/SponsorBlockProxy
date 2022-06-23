# syntax=docker/dockerfile:1
#FROM mcr.microsoft.com/dotnet/aspnet:6.0
FROM localhost/aspnetffmpeg

COPY SponsorBlockProxy.Web/bin/Debug/net6.0/publish/ App/

ENV SBP_CONFIG_DIR="/config"

WORKDIR /App
ENTRYPOINT ["dotnet", "SponsorBlockProxy.Web.dll"]
