#! /bin/bash

set -e

dotnet publish SponsorBlockProxy.Web

rm -rf SponsorBlockProxy.Web/bin/Debug/net6.0/publish/Samples/
rm SponsorBlockProxy.Web/bin/Debug/net6.0/publish/appsettings.*

podman build -t sponsorblockproxy .

#podman run --replace -p 3500:80 --name podcastblock sponsorblockproxy:latest