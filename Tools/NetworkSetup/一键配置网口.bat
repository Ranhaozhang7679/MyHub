@echo off
chcp 65001 >nul 2>&1
title 一键配置网口 - LusterMotion

:: 检查是否以管理员身份运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo   需要管理员权限，正在请求提权...
    echo.
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: 以管理员身份运行 PowerShell 脚本
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup_network.ps1"

pause
