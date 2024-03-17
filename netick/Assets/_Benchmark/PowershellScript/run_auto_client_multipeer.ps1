# Prompt the user for the client count
$clientCount = Read-Host "Enter the number of clients"
$serverIP = Read-Host "Enter the Server IP Address (default is 127.0.0.1)"

if($serverIP -eq "") {
    $serverIP = "127.0.0.1"
}

# Validate and convert the input to an integer
while (-not ($clientCount -as [int])) {
    Write-Host "Invalid input. Please enter a valid integer."
    $clientCount = Read-Host "Enter the number of clients"
}

# Run the executable with the specified arguments
Start-Process -FilePath "netcode-benchmark.exe" -ArgumentList "-autoclient", "-clientcount", "$clientCount", "-serverip", "$serverIP"