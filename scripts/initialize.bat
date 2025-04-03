@echo off
docker-compose build
docker-compose run --rm ollama scripts/pull-model.bat
pause
