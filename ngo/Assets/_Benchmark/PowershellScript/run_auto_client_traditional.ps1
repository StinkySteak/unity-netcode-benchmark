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

for ($i = 1; $i -le $clientCount; $i++) {
    Start-Process -FilePath "ngo.exe" -ArgumentList "-autoclient", "-clientcount", "1", "-serverip", "$serverIP"
}