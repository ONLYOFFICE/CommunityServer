@echo off

echo Moving to release directory
cd Nuget\Content

echo Packing nuget package
..\nuget pack ..\SharpBox.nuspec -Version %1

echo Pushing nuget package 
..\nuget Push AppLimit.CloudComputing.SharpBox.%1.nupkg

echo Leaving directories
cd ..\..