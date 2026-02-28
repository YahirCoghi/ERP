@echo off
curl.exe -i -X POST "http://localhost:5000/api/auth/register" -H "Content-Type: application/json" -d "{\"Username\":\"admin@minierp.com\",\"Password\":\"Admin123!\",\"Email\":\"admin@minierp.com\"}"
pause
