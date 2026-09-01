@echo off
setlocal

set "PROJECT_ROOT=%~dp0"
set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"

echo ========================================
echo SMC Bhoomi Vyavasthapan
echo ========================================
echo Stopping existing project listeners on ports 3000 and 5072...

for %%P in (3000 5072) do (
    for /f "tokens=5" %%I in ('netstat -ano ^| findstr /R /C:":%%P .*LISTENING"') do (
        echo Stopping PID %%I on port %%P...
        taskkill /PID %%I /F >nul 2>&1
    )
)

echo Starting Backend...
start "SMC Backend" /D "%PROJECT_ROOT%" cmd.exe /k "set ASPNETCORE_ENVIRONMENT=Development && dotnet run --project backend\src\SMC.API\SMC.API.csproj --no-launch-profile --urls http://localhost:5072"

echo Starting Frontend...
start "SMC Frontend" /D "%PROJECT_ROOT%\frontend" cmd.exe /k "npm run dev"

echo Backend: http://localhost:5072
echo Frontend: http://localhost:3000
endlocal
