@echo off
setlocal EnableExtensions

set "PROJECT_ROOT=%~dp0"
set "BACKEND_PROJECT=%PROJECT_ROOT%backend\src\SMC.API\SMC.API.csproj"
set "FRONTEND_DIR=%PROJECT_ROOT%frontend"

echo.
echo ========================================
echo SMC Bhoomi Vyavasthapan - Portable Start
echo ========================================
echo.

where dotnet >nul 2>&1 || (
    echo ERROR: .NET SDK is not installed or is not available on PATH.
    exit /b 1
)
where node >nul 2>&1 || (
    echo ERROR: Node.js is not installed or is not available on PATH.
    exit /b 1
)
where npm.cmd >nul 2>&1 || (
    echo ERROR: npm is not installed or is not available on PATH.
    exit /b 1
)

if not exist "%BACKEND_PROJECT%" (
    echo ERROR: Backend project was not found: %BACKEND_PROJECT%
    exit /b 1
)
if not exist "%FRONTEND_DIR%\package.json" (
    echo ERROR: Frontend package.json was not found: %FRONTEND_DIR%\package.json
    exit /b 1
)

echo Restoring backend dependencies...
dotnet restore "%BACKEND_PROJECT%"
if errorlevel 1 (
    echo ERROR: Backend dependency restore failed.
    exit /b 1
)

if not exist "%FRONTEND_DIR%\node_modules\" (
    echo Restoring frontend dependencies for this fresh checkout...
    pushd "%FRONTEND_DIR%"
    if exist "package-lock.json" (
        call npm.cmd ci
    ) else (
        call npm.cmd install
    )
    if errorlevel 1 (
        popd
        echo ERROR: Frontend dependency restore failed.
        exit /b 1
    )
    popd
) else (
    echo Frontend dependencies are already present.
)

echo Starting services and checking ports/readiness...
call "%PROJECT_ROOT%run-project.bat"
set "START_RESULT=%ERRORLEVEL%"

if not "%START_RESULT%"=="0" (
    echo ERROR: SMC Bhoomi Vyavasthapan did not start successfully.
    exit /b %START_RESULT%
)

echo.
echo Backend: http://localhost:5072
echo Frontend: http://localhost:3000
echo Database migrations and idempotent seed data are applied by backend startup.
endlocal
exit /b 0
