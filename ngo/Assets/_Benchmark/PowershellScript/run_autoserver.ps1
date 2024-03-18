$testNumber = Read-Host "Enter the test number (1, 2, 3)"

while (-not ($testNumber -as [int])) {
    Write-Host "Invalid input. Please enter a valid integer."
    $testNumber = Read-Host "Enter the test number (1, 2, 3)"
}

Start-Process -FilePath "ngo.exe" -ArgumentList "-autoserver", "-batchmode", "-nographics","-test$testNumber"