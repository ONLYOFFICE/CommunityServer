set Constants=DEBUGINFO
"%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" msbuild\build.proj /fl1 /flp1:LogFile=Build.log;Verbosity=Normal /m -tv:Current -v:m
pause