#!/bin/sh
export MONO_IOMAP=all
VSToolsPath=msbuild
msbuild msbuild/build.proj /flp:LogFile=Build.log
