#!/bin/sh
export MONO_IOMAP=all
export VSToolsPath=msbuild
export DeployTo=$1

xbuild msbuild/build.proj /flp:LogFile=Build.log
xbuild msbuild/deploy.proj /flp:LogFile=Deploy.log
