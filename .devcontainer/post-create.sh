#!/bin/sh

## Create a k3d cluster
while (! kubectl cluster-info ); do
  # Docker takes a few seconds to initialize
  echo "Waiting for Docker to launch..."
  k3d cluster delete
  k3d registry create myregistry.localhost --port 12345
  k3d cluster create -p '8081:80@loadbalancer' --k3s-arg '--disable=traefik@server:0' --registry-use k3d-myregistry.localhost:12345
  sleep 1
done

# docker pull docker.io/library/postgres:16.2
# docker pull docker.io/library/rabbitmq:3.13-management
# docker pull docker.io/library/redis:7.2

# docker tag docker.io/library/postgres:16.2 k3d-myregistry.localhost:12345/postgres:16.2
# docker tag docker.io/library/rabbitmq:3.13-management k3d-myregistry.localhost:12345/rabbitmq:3.13-management
# docker tag docker.io/library/redis:7.2 k3d-myregistry.localhost:12345/redis:7.2

# docker tag product-api k3d-myregistry.localhost:12345/product-api:latest

## dotnet
dotnet restore

## docker-compose
# docker compose -f ./docker-compose.yaml up -d
