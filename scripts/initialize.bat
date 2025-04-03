@echo off
docker-compose down
docker-compose build
docker-compose run --rm ollama pull-model.bat
pause
