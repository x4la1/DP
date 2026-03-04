$ProjectPath = "C:\Users\Lebed\source\repos\DP\Valuator"
$Ports = 5001, 5002, 5003

Write-Host "--- Starting Valuator instances ---" -ForegroundColor Cyan

foreach ($Port in $Ports) {
    Write-Host "Launching instance on port $Port..."
    Start-Process dotnet -ArgumentList "run --project `"$ProjectPath`" --urls http://localhost:$Port" -WindowStyle Minimized
}

Write-Host "--- Starting Nginx ---" -ForegroundColor Cyan
Start-Process "C:\nginx\nginx.exe" -WorkingDirectory "C:\nginx\"

Write-Host "Done! All components are launching." -ForegroundColor Green