#!/bin/bash

# Create key pair
mkdir -p ../KeyPair
openssl genrsa -out ../KeyPair/private_key.pem 2048
openssl rsa -in ../KeyPair/private_key.pem -pubout -out ../KeyPair/public_key.pem

# Start up docker compose
docker compose -f ../docker-compose-dev.yml -p twiker-docker up -d --remove-orphans