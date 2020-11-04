set Constants=DEBUGINFO
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" msbuild\build.proj /fl1 /flp1:LogFile=Build.log;Verbosity=Normal /m -tv:Current -v:m
pause