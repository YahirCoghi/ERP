param(
    [string]$ApiUrl = 'http://localhost:5000',
    [string]$Username = 'admin@minierp.com',
    [string]$Password = 'Admin123!',
    [string]$Email = 'admin@minierp.com'
)

Write-Host "Seeding admin user to $ApiUrl..."
for ($i=0; $i -lt 30; $i++) {
    try {
        $resp = Invoke-RestMethod -Uri "$ApiUrl/api/auth/register" -Method Post -ContentType 'application/json' -Body (@{ Username=$Username; Password=$Password; Email=$Email } | ConvertTo-Json) -ErrorAction Stop
        Write-Host "Success: $($resp | ConvertTo-Json -Depth 3)"
        exit 0
    } catch {
        Write-Host "Attempt $($i+1): API not ready, waiting..."
        Start-Sleep -Seconds 2
    }
}
Write-Error "Failed to seed admin: API did not become ready in time."
