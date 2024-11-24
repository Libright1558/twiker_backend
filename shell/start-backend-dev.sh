#!/bin/bash

# Start up docker compose
docker compose -f ./docker-compose-dev.yml -p twiker-docker up -d --remove-orphans

# Disable nginx default.conf
docker exec nginx-container mv /etc/nginx/conf.d/default.conf /etc/nginx/conf.d/default.conf.disabled

# Reload nginx
docker exec nginx-container nginx -s reload