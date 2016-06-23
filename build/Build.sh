#!/bin/sh
export MONO_IOMAP=all
VSToolsPath=msbuild
xbuild msbuild/build.proj /flp:LogFile=Build.log
