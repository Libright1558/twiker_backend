#!/bin/bash

# Build twiker-app image
docker build . -t libright1558/twiker-app-dev:v1.0.0 -f twiker-app-dev.dockerfile 