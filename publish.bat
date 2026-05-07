@echo off
:: WinCal 一键发布脚本
:: 输出: ./dist/WinCal.exe (单文件，约 35~55 MB)

echo ========================================
echo   WinCal 发布脚本
echo ========================================
echo.

:: 清理旧的发布产物
if exist ".\dist" (
    echo [1/3] 清理旧的发布产物...
    rmdir /s /q ".\dist"
) else (
    echo [1/3] 无需清理
)

:: 发布
echo.
echo [2/3] 开始发布 (Release, win-x64, Self-contained, SingleFile)...
dotnet publish WinCal.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:EnableCompressionInSingleFile=true ^
  -o ./dist

if %ERRORLEVEL% neq 0 (
    echo.
    echo [错误] 发布失败！请检查错误信息。
    pause
    exit /b 1
)

:: 清理调试符号（可选）
echo.
echo [3/3] 清理调试符号...
if exist ".\dist\WinCal.pdb" del ".\dist\WinCal.pdb"

:: 显示结果
echo.
echo ========================================
echo   发布完成！
echo ========================================
echo.
echo 输出目录: %cd%\dist\
echo.

:: 显示文件大小
for %%F in (".\dist\WinCal.exe") do (
    set SIZE=%%~zF
)
echo WinCal.exe 大小: %SIZE% 字节
echo.
pause
