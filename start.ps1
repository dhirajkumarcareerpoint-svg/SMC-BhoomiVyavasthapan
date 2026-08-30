$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$backendProject = Join-Path $root 'backend\src\SMC.API\SMC.API.csproj'
$frontendPath = Join-Path $root 'frontend'

function Test-PortInUse([int] $port) {
    return $null -ne (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
}

if (-not (Test-PortInUse 5000)) {
    Start-Process powershell -WorkingDirectory (Split-Path $backendProject) -ArgumentList @(
        '-NoExit', '-NoProfile', '-Command', 'dotnet run --no-launch-profile --urls http://localhost:5000'
    )
}

if (-not (Test-PortInUse 5173)) {
    Start-Process powershell -WorkingDirectory $frontendPath -ArgumentList @(
        '-NoExit', '-NoProfile', '-Command', 'npm run dev -- --host localhost --port 5173'
    )
}

Write-Host 'Backend:  http://localhost:5000'
Write-Host 'Frontend: http://localhost:5173'
Write-Host 'Swagger:  http://localhost:5000/swagger'
