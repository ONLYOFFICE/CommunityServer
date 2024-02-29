@echo off

PUSHD %~dp0..

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.xml) do (
		call TeamLabSvc\WinSW3.0.0.exe install %%f
	)
)

echo.
pause