$uri='http://localhost:5000/api/products'
for ($i=0; $i -lt 30; $i++) {
    try {
        $r = Invoke-RestMethod -Uri $uri -Method Get -TimeoutSec 2
        break
    } catch {
        Start-Sleep -Seconds 1
    }
}
if (-not $r) { Write-Error 'API not reachable'; exit 1 }

$body = @{
    name = 'E2E Test Product'
    price = 19.99
    cost = 10.00
    stock = 5
    minStock = 1
} | ConvertTo-Json

$prod = Invoke-RestMethod -Uri $uri -Method Post -Body $body -ContentType 'application/json'
Write-Output "Created product: $($prod.id) $($prod.code)"

$txBody = @{
    type = 'Expense'
    amount = 5.00
    description = 'E2E test expense'
} | ConvertTo-Json

$tx = Invoke-RestMethod -Uri "http://localhost:5000/api/products/$($prod.id)/transactions" -Method Post -Body $txBody -ContentType 'application/json'
Write-Output "Created transaction: $($tx.id) $($tx.amount)"

$txs = Invoke-RestMethod -Uri "http://localhost:5000/api/products/$($prod.id)/transactions" -Method Get
$txs | ConvertTo-Json -Depth 5
