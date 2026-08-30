@echo off
setlocal EnableExtensions

set "ROOT=%~dp0"
set "BACKEND_DIR=%ROOT%backend\src\SMC.API"
set "FRONTEND_DIR=%ROOT%frontend"
set "SMC_PROJECT_ROOT=%ROOT:~0,-1%"
set "CLEAR_NEXT_CACHE=0"

echo.
echo ========================================
echo SMC Bhoomi Vyavasthapan
echo ========================================
echo.

if not exist "%BACKEND_DIR%\SMC.API.csproj" (
    echo ERROR: Backend project not found:
    echo %BACKEND_DIR%\SMC.API.csproj
    exit /b 1
)

if not exist "%FRONTEND_DIR%\package.json" (
    echo ERROR: Frontend package.json not found:
    echo %FRONTEND_DIR%\package.json
    exit /b 1
)

call :PreparePort 5072 Backend
if errorlevel 1 exit /b 1

call :PreparePort 3000 Frontend
if errorlevel 1 exit /b 1

if "%CLEAR_NEXT_CACHE%"=="1" if exist "%FRONTEND_DIR%\.next" (
    echo Clearing only the stale frontend .next cache...
    rmdir /s /q "%FRONTEND_DIR%\.next"
)

echo.
echo Starting Backend...
echo Backend: http://localhost:5072
echo Swagger: http://localhost:5072/swagger
start "SMC Bhoomi API [SMC_BHOOMI]" /D "%BACKEND_DIR%" cmd.exe /k "title SMC Bhoomi API [SMC_BHOOMI]&& set ASPNETCORE_ENVIRONMENT=Development&& set SMC_BHOOMI_PROJECT=1&& dotnet run --project SMC.API.csproj --no-launch-profile --urls http://localhost:5072"

echo.
echo Starting Frontend...
echo Frontend: http://localhost:3000
start "SMC Bhoomi Frontend [SMC_BHOOMI]" /D "%FRONTEND_DIR%" cmd.exe /k "title SMC Bhoomi Frontend [SMC_BHOOMI]&& set SMC_BHOOMI_PROJECT=1&& npm run dev"

echo.
echo Waiting for Backend and Frontend to become ready...
call :WaitForUrl "http://localhost:5072/swagger" 45
if errorlevel 1 (
    echo ERROR: Backend did not become ready at http://localhost:5072/swagger within 45 seconds.
    echo Check the SMC Bhoomi API window for details.
    endlocal
    exit /b 1
)

call :WaitForUrl "http://localhost:3000" 60
if errorlevel 1 (
    echo ERROR: Frontend did not become ready at http://localhost:3000 within 60 seconds.
    echo Check the SMC Bhoomi Frontend window for details.
    endlocal
    exit /b 1
)

echo Opening the SMC Bhoomi Vyavasthapan application...
start "" "http://localhost:3000"

echo SMC Bhoomi Vyavasthapan is ready.
echo Main application: http://localhost:3000
echo Swagger (development only): http://localhost:5072/swagger
echo Do not close the Backend or Frontend windows while developing.
echo.
endlocal
exit /b 0

:WaitForUrl
set "SMC_WAIT_URL=%~1"
set "SMC_WAIT_SECONDS=%~2"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$deadline=(Get-Date).AddSeconds([int]$env:SMC_WAIT_SECONDS); do { try { $response=Invoke-WebRequest -Uri $env:SMC_WAIT_URL -UseBasicParsing -TimeoutSec 3; if($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) { exit 0 } } catch { }; Start-Sleep -Milliseconds 500 } while((Get-Date) -lt $deadline); exit 1"
exit /b %errorlevel%

:PreparePort
set "SMC_PORT=%~1"
set "SMC_KIND=%~2"
if /I "%SMC_KIND%"=="Backend" (set "SMC_WINDOW_TITLE=SMC Bhoomi API [SMC_BHOOMI]") else (set "SMC_WINDOW_TITLE=SMC Bhoomi Frontend [SMC_BHOOMI]")
set "PORT_RESULT="
set "SMC_PID="
for /f "tokens=5" %%P in ('netstat -ano ^| findstr /R /C:":%SMC_PORT% .*LISTENING"') do if not defined SMC_PID set "SMC_PID=%%P"

if not defined SMC_PID (
    set "PORT_RESULT=FREE"
) else (
    set "SMC_PROCESS_ID=%SMC_PID%"
    for /f "usebackq delims=" %%R in (`powershell -NoProfile -ExecutionPolicy Bypass -Command "$p=Get-CimInstance Win32_Process -Filter ('ProcessId=' + $env:SMC_PROCESS_ID) -ErrorAction Stop; $root=[string]$env:SMC_PROJECT_ROOT; $marker=[string]$env:SMC_WINDOW_TITLE; $known=$false; for($i=0; $i -lt 8 -and $p; $i++){ if(([string]$p.CommandLine) -like ('*' + $root + '*')) { $known=$true; break }; try { if((Get-Process -Id $p.ProcessId -ErrorAction Stop).MainWindowTitle -eq $marker) { $known=$true; break } } catch {}; if($p.ParentProcessId -le 0){break}; $p=Get-CimInstance Win32_Process -Filter ('ProcessId=' + $p.ParentProcessId) -ErrorAction SilentlyContinue }; if($known) { Stop-Process -Id ([int]$env:SMC_PROCESS_ID) -Force -ErrorAction Stop; 'PROJECT_STOPPED' } else { 'IN_USE:' + $env:SMC_PROCESS_ID }" 2^>nul`) do set "PORT_RESULT=%%R"
    if not defined PORT_RESULT set "PORT_RESULT=IN_USE:%SMC_PID%"
)

if /I "%PORT_RESULT%"=="FREE" (
    echo %SMC_KIND% port %SMC_PORT% is available.
    exit /b 0
)

if /I "%PORT_RESULT%"=="PROJECT_STOPPED" (
    echo Stopped an old SMC Bhoomi %SMC_KIND% process on port %SMC_PORT%.
    if /I "%SMC_KIND%"=="Frontend" set "CLEAR_NEXT_CACHE=1"
    timeout /t 2 /nobreak >nul
    exit /b 0
)

echo ERROR: Port %SMC_PORT% is already in use by process %PORT_RESULT:IN_USE:=%. 
echo It was not stopped because it could not be proven to belong to this SMC Bhoomi project.
echo Close that process or change nothing and run this script again.
exit /b 1
