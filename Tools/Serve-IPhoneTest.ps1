param(
    [ValidateRange(1024,65535)][int]$Port = 8080,
    [switch]$AllowLocalNetwork,
    [string]$LocalAddress = ''
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent $PSScriptRoot
$buildIndex = Join-Path $repository 'Android Puzzle/Builds/iPhoneWebTest/index.html'
if (-not (Test-Path -LiteralPath $buildIndex)) { throw 'Build the Unity web test first.' }
$serverScript = Join-Path $PSScriptRoot 'serve_iphone_test.py'
$pythonPath = (Get-Command python -ErrorAction Stop).Source
$logDirectory = Join-Path $repository 'Android Puzzle/Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$stateFile = Join-Path $logDirectory 'iphone-web-server.json'
if (Test-Path -LiteralPath $stateFile) { throw 'A server state file already exists. Stop the existing server first; see README for crash recovery.' }
$stopFile = Join-Path $logDirectory 'iphone-web-test.stop'
$ruleName = 'Puzzle iPhone Web Test ' + $PID
$ruleCreated = $false
$bindAddress = '127.0.0.1'

if ($AllowLocalNetwork) {
    $principal = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Run this script as Administrator with -AllowLocalNetwork to create the approved temporary firewall rule.'
    }
    $parsedAddress = $null
    if (-not [System.Net.IPAddress]::TryParse($LocalAddress, [ref]$parsedAddress) -or
        $parsedAddress.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
        throw 'Pass the current PC IPv4 address using -LocalAddress (see ipconfig).'
    }
    $bindAddress = $LocalAddress
}

if (Test-Path -LiteralPath $stopFile) { Remove-Item -LiteralPath $stopFile }
try {
    if ($AllowLocalNetwork) {
        & netsh advfirewall firewall add rule "name=$ruleName" dir=in action=allow protocol=TCP "localport=$Port" "localip=$LocalAddress" remoteip=localsubnet "program=$pythonPath" profile=private,public enable=yes edge=no
        if ($LASTEXITCODE -ne 0) { throw 'Could not create the temporary firewall rule.' }
        $ruleCreated = $true
    }
    $serverInfo = @{ port=$Port; address=$LocalAddress; firewallRule= $(if ($ruleCreated) {$ruleName} else {''}); processId=$PID }
    $serverInfo | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $logDirectory 'iphone-web-server.json') -Encoding utf8
    Write-Host ('PC preview: http://' + $bindAddress + ':' + $Port + '/')
    if ($AllowLocalNetwork) { Write-Host ('iPhone Safari: http://' + $LocalAddress + ':' + $Port + '/') }
    Write-Host 'Stop with Tools/Stop-IPhoneTest.ps1 or Ctrl+C. The temporary firewall rule is removed on exit.'
    & $pythonPath $serverScript --port $Port --bind $bindAddress --stop-file $stopFile
    if ($LASTEXITCODE -ne 0) { throw 'The web test server exited with an error.' }
}
finally {
    if ($ruleCreated) {
        & netsh advfirewall firewall delete rule "name=$ruleName" | Out-Null
        if ($LASTEXITCODE -ne 0) { throw ('Firewall cleanup failed. Remove rule manually: ' + $ruleName) }
    }
    if (Test-Path -LiteralPath $stopFile) { Remove-Item -LiteralPath $stopFile }
    $stateFile = Join-Path $logDirectory 'iphone-web-server.json'
    if (Test-Path -LiteralPath $stateFile) { Remove-Item -LiteralPath $stateFile }
}


