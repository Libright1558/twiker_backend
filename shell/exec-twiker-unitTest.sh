#!/bin/bash

docker exec -w /source twiker-app-unitTest dotnet test --no-restore --no-build