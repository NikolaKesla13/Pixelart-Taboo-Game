@echo off
TITLE Tabu F2 Launcher
COLOR 0A

:: 1. Dosyanin oldugu klasore git
cd /d "%~dp0"

ECHO ==========================================
ECHO      P T A B U   F 2   BASLATILIYOR
ECHO ==========================================
ECHO.
ECHO Oyun ZORLA https://localhost:7261 adresinde aciliyor...
ECHO.

:: 2. Tarayiciyi ac (Biraz bekleyip)
timeout /t 5 >nul
start https://localhost:7261

:: 3. OYUNU ZORLA BU PORTTA BASLAT (--urls komutu ile)
dotnet run --urls "https://localhost:7261"

pause