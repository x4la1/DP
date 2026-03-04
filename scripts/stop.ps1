Write-Host "--- Stopping Nginx ---" -ForegroundColor Yellow
Start-Process "C:\nginx\nginx.exe" -ArgumentList "-s stop" -WorkingDirectory "C:\nginx\"

Write-Host "--- Stopping Valuator instances ---" -ForegroundColor Yellow

$Ports = 5001, 5002, 5003
foreach ($Port in $Ports) {
    $ProcessId = (Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue).OwningProcess
    if ($ProcessId) {
        Stop-Process -Id $ProcessId
        Write-Host "Stopped process on port $Port"
    }
}

Write-Host "Cleanup complete." -ForegroundColor Green