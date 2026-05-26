Write-Host "Stopping Nginx" -ForegroundColor Yellow
Start-Process "C:\nginx\nginx.exe" -ArgumentList "-s stop" -WorkingDirectory "C:\nginx\"

Write-Host "Stopping Valuator instances" -ForegroundColor Yellow
$Ports = 5001, 5002, 5003
foreach ($Port in $Ports) {
    $ProcessId = (Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue).OwningProcess
    if ($ProcessId) {
        Stop-Process -Id $ProcessId -Force
        Write-Host "Stopped Valuator on port $Port"
    }
}

Write-Host "Stopping RankCalculator instances" -ForegroundColor Yellow
$calcProcesses = Get-Process -Name "RankCalculator" -ErrorAction SilentlyContinue
if ($calcProcesses) {
    Stop-Process -InputObject $calcProcesses -Force
    Write-Host "Stopped $($calcProcesses.Count) RankCalculator instance(s)."
} else {
    Write-Host "No RankCalculator processes found."
}

Write-Host "Stopping EventsLogger instances" -ForegroundColor Yellow
$logProcesses = Get-Process -Name "EventsLogger" -ErrorAction SilentlyContinue
if ($logProcesses) {
    Stop-Process -InputObject $logProcesses -Force
    Write-Host "Stopped $($logProcesses.Count) EventsLogger instance(s)."
}

Write-Host "Stopping Redis instances" -ForegroundColor Yellow
$redisProcesses = Get-Process -Name "redis-server" -ErrorAction SilentlyContinue

if ($redisProcesses) 
{
    Stop-Process -InputObject $redisProcesses -Force
    Write-Host "Stopped $($redisProcesses.Count) Redis instance(s)."
}
else 
{
    Write-Host "No Redis processes found."
}

Write-Host "Cleanup complete" -ForegroundColor Green