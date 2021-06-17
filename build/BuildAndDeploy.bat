set Cultures=fr,de,es,ru,lv,pt-BR,pt,it,tr,el,zh-CN,pl,cs,uk,vi,fi,az-Latn-AZ,ko,ja,sl,sk,nl,bg

set BuildTargets=ReBuild
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" msbuild\build.proj /fl1 /flp1:LogFile=Build.log;Verbosity=Normal /m -v:m
set BuildTargets=Build
if %errorlevel% == 0 "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" msbuild\deploy.proj /fl1 /flp1:LogFile=Deploy.log;Verbosity=Normal /m -v:m
pause
