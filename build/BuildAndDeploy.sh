#!/bin/sh
export MONO_IOMAP=all
export VSToolsPath=msbuild
export DeployTo=$1

msbuild msbuild/build.proj /flp:LogFile=Build.log
msbuild msbuild/deploy.proj /flp:LogFile=Deploy.log
