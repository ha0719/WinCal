@echo off
cd /d "%~dp0"
"C:\Program Files\dotnet\dotnet.exe" build -c Debug 2>&1
