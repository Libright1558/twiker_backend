#!/bin/bash

docker compose  -f ./docker-compose-unittest.yml -p twiker-docker down --remove-orphans 

docker rmi twiker-app-unittest