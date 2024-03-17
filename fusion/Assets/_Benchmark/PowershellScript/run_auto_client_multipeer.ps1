# Prompt the user for the client count
$clientCount = Read-Host "Enter the number of clients"

# Validate and convert the input to an integer
while (-not ($clientCount -as [int])) {
    Write-Host "Invalid input. Please enter a valid integer."
    $clientCount = Read-Host "Enter the number of clients"
}

# Run the executable with the specified arguments
Start-Process -FilePath "fusion.exe" -ArgumentList "-autoclient", "-clientcount", "$clientCount"