#!/bin/bash

# Start up docker compose
docker compose -f ./docker-compose-unittest.yml -p twiker-docker up -d --remove-orphans