%SystemRoot%\Microsoft.NET\Framework\v3.5\MSBuild.exe .\SQLite.Interop\SQLite.Interop.2008.vcproj /p:Configuration=ReleaseNativeOnly;Platform=Win32
%SystemRoot%\Microsoft.NET\Framework\v3.5\MSBuild.exe .\SQLite.Interop\SQLite.Interop.2008.vcproj /p:Configuration=ReleaseNativeOnly;Platform=x64
%SystemRoot%\Microsoft.NET\Framework\v3.5\MSBuild.exe .\System.Data.SQLite\System.Data.SQLite.2008.csproj /p:Configuration=Release;UseIneropDll=true

xcopy .\bin\2008\Release\bin\System.Data.SQLite.dll ..\ /Y
xcopy .\bin\2008\Release\bin\System.Data.SQLite.xml ..\ /Y
xcopy .\bin\2008\Win32\ReleaseNativeOnly\SQLite.Interop.dll ..\x86\ /Y
xcopy .\bin\2008\x64\ReleaseNativeOnly\SQLite.Interop.dll ..\x64\ /Y