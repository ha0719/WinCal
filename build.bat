@echo off
cd /d "%~dp0"
"C:\Program Files\dotnet\dotnet.exe" build 2>&1
pause
