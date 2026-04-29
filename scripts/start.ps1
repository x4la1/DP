$ValuatorPath = "C:\Users\Lebed\source\repos\DP\Valuator"
$CalcPath = "C:\Users\Lebed\source\repos\DP\RankCalculator"
$Ports = 5001, 5002, 5003

Write-Host "--- Starting Valuator instances ---" -ForegroundColor Cyan
foreach ($Port in $Ports) {
    Write-Host "Launching Valuator on port $Port..."
    Start-Process dotnet -ArgumentList "run --project `"$ValuatorPath`" --urls http://localhost:$Port" -WindowStyle Minimized
}

Write-Host "--- Starting RankCalculators (Consumers) ---" -ForegroundColor Cyan
for ($i=1; $i -le 2; $i++) {
    Write-Host "Launching RankCalculator instance $i..."
    Start-Process dotnet -ArgumentList "run --project `"$CalcPath`"" -WindowStyle Minimized
}

Write-Host "--- Starting Nginx ---" -ForegroundColor Cyan
Start-Process "C:\nginx\nginx.exe" -WorkingDirectory "C:\nginx\"

Write-Host "Done! All components are running." -ForegroundColor Green