%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe msbuild\build.proj /p:BuildTargets=ReBuild /fl1 /flp1:LogFile=Build.log;Verbosity=Normal /m -tv:14.0
if %errorlevel% == 0 %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe msbuild\deploy.proj /fl1 /flp1:LogFile=Deploy.log;Verbosity=Normal /m -tv:14.0
pause
