#!/bin/bash

docker compose  -f ./docker-compose-dev.yml -p twiker-docker down --remove-orphans

# Remove .env
rm .env

# Remove key pair
rm -rf ./KeyPair