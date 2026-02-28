@echo off
curl.exe -i -X POST "http://localhost:5000/api/auth/login" -H "Content-Type: application/json" -d "{\"Username\":\"admin@minierp.com\",\"Password\":\"Admin123!\"}"
pause
