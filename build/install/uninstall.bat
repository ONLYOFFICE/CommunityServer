@echo off

PUSHD %~dp0..

if %errorlevel% == 0 (
	for /R "run\" %%f in (*.xml) do (
		call TeamLabSvc\WinSW3.0.0.exe stop %%f
		call TeamLabSvc\WinSW3.0.0.exe uninstall %%f
	)
)

echo.
pause