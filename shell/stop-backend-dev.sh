#!/bin/bash

docker compose  -f ./docker-compose-dev.yml -p twiker-docker down --remove-orphans
