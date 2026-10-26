$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $repository 'Android Puzzle/Logs'
$stateFile = Join-Path $logs 'iphone-web-server.json'
if (-not (Test-Path -LiteralPath $stateFile)) { Write-Host 'No managed iPhone test server is running.'; return }
Set-Content -LiteralPath (Join-Path $logs 'iphone-web-test.stop') -Value 'stop' -Encoding ascii
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    if (-not (Test-Path -LiteralPath $stateFile)) {
        Write-Host 'Server stopped and temporary firewall rule removed.'
        return
    }
    Start-Sleep -Milliseconds 500
}
throw 'The server did not confirm cleanup. Check iphone-web-server.json and the crash recovery steps in README.'
