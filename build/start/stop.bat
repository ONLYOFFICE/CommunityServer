@echo off

PUSHD %~dp0..

POPD

if %errorlevel% == 0 (
	pwsh  %~dp0/command.ps1 "stop"
)

echo.

if "%1"=="nopause" goto start
pause
:start