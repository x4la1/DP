$ValuatorPath = "C:\Users\Денис\source\repos\DP\Valuator"
$CalcPath = "C:\Users\Денис\source\repos\DP\RankCalculator"
$LoggerPath = "C:\Users\Денис\source\repos\DP\EventsLogger"
$Ports = 5001, 5002, 5003

Write-Host "Starting Valuator instances" -ForegroundColor Cyan
foreach ($Port in $Ports) {
    Write-Host "Launching Valuator on port $Port"
    Start-Process dotnet -ArgumentList "run --project `"$ValuatorPath`" --urls http://localhost:$Port" -WindowStyle Minimized
}

Write-Host "Starting RankCalculators" -ForegroundColor Cyan
for ($i=1; $i -le 2; $i++) {
    Write-Host "Launching RankCalculator instance $i"
    Start-Process dotnet -ArgumentList "run --project `"$CalcPath`"" -WindowStyle Minimized
}

Write-Host "Starting EventsLoggers" -ForegroundColor Cyan
for ($i=1; $i -le 2; $i++) {
    Write-Host "Launching EventsLogger instance $i"
    Start-Process dotnet -ArgumentList "run --project `"$LoggerPath`"" -WindowStyle Minimized
}

Write-Host "Starting Nginx" -ForegroundColor Cyan
Start-Process "C:\nginx\nginx.exe" -WorkingDirectory "C:\nginx\"

Write-Host "Done." -ForegroundColor Green