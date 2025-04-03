@echo off
setlocal enabledelayedexpansion

:: Set colors for better readability
set "BLUE=[94m"
set "GREEN=[92m"
set "YELLOW=[93m"
set "RED=[91m"
set "RESET=[0m"

:: Get the model name from environment variable or command line or use default
if "%OLLAMA_MODELS%"=="" (
    set MODEL=%1
    if "%MODEL%"=="" set MODEL=codellama
) else (
    set MODEL=%OLLAMA_MODELS%
)

echo %BLUE%Pulling LLM model: %MODEL%...%RESET%
echo.

:: Determine which port Ollama is using
set OLLAMA_PORT=11434
for /f "tokens=*" %%a in ('docker-compose ps ^| findstr "ollama" ^| findstr -i "Up" ^| findstr -i "->11434"') do (
    for /f "tokens=3 delims=:" %%b in ("%%a") do (
        for /f "tokens=1 delims=-" %%c in ("%%b") do (
            set OLLAMA_PORT=%%c
        )
    )
)

:: Wait for Ollama to be ready
echo %YELLOW%Waiting for Ollama service to be ready...%RESET%
set RETRY_COUNT=0
set MAX_RETRIES=30

:wait_loop
timeout /t 2 /nobreak > nul
curl -s -f http://localhost:%OLLAMA_PORT%/api/version > nul
if %ERRORLEVEL% NEQ 0 (
    echo %YELLOW%.%RESET%
    set /a RETRY_COUNT+=1
    if %RETRY_COUNT% GEQ %MAX_RETRIES% (
        echo %RED%Timed out waiting for Ollama service to be ready.%RESET%
        echo %YELLOW%Continuing anyway, but the model pull may fail.%RESET%
        echo.
        goto pull_model
    )
    goto wait_loop
)
echo %GREEN%Ollama service is ready.%RESET%
echo.

:pull_model

:: Pull the model
echo %BLUE%Pulling model %MODEL%...%RESET%
curl -X POST http://localhost:%OLLAMA_PORT%/api/pull -d "{\"name\":\"%MODEL%\"}"
echo.

:: Verify the model is available
echo %YELLOW%Verifying model availability...%RESET%
set RETRY_COUNT=0
set MAX_RETRIES=10

:verify_loop
timeout /t 2 /nobreak > nul
curl -s -X POST http://localhost:%OLLAMA_PORT%/api/generate -d "{\"model\":\"%MODEL%\", \"prompt\":\"test\", \"stream\":false}" > nul
if %ERRORLEVEL% NEQ 0 (
    echo %YELLOW%.%RESET%
    set /a RETRY_COUNT+=1
    if %RETRY_COUNT% GEQ %MAX_RETRIES% (
        echo %RED%Timed out waiting for model to be available.%RESET%
        echo %YELLOW%You may need to wait longer for the model to finish downloading.%RESET%
        echo.
        goto end
    )
    goto verify_loop
)
echo %GREEN%Model %MODEL% is available and ready to use.%RESET%

:end

pause
endlocal
