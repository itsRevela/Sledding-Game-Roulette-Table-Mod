@echo off
rem Roulette Revival installer: sets up MelonLoader and builds the mod against your copy of Sledding Game.
rem Optional: install.bat -GameDir "D:\path\to\Sledding Game"
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\install.ps1" %*
set "EXITCODE=%ERRORLEVEL%"
echo.
pause
exit /b %EXITCODE%
