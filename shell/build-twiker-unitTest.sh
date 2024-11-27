#!/bin/bash

# Build twiker unit tests image
docker build . -t twiker-app-unittest -f twiker-app-unittest.dockerfile