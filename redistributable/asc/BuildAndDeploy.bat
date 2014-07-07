set Configuration=Release
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ..\..\_ci\msbuild\build.proj /p:BuildTargets=ReBuild /fl1 /flp1:LogFile=Build.log;Verbosity=Normal
if %errorlevel% == 0 %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ..\..\_ci\msbuild\deploy.proj /fl1 /flp1:LogFile=Deploy.log;Verbosity=Normal
pause
