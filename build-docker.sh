#! /bin/bash

set -e

dotnet publish SponsorBlockProxy.Web

podman build -t sponsorblockproxy .

podman run --replace -p 3500:80 --name podcastblock sponsorblockproxy:latest