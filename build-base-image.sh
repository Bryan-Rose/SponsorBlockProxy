#! /bin/bash

set -e


podman build -t aspnetffmpeg -f Dockerfile-netffmpeg

#podman run --replace -p 3500:80 --name podcastblock sponsorblockproxy:latest