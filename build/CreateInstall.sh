#!/bin/sh
export MONO_IOMAP=all
VSToolsPath=msbuild
Configuration=Release
DeployTo=UNIX.SERVER

set -e

PLATFORM=""
if [ `python -mplatform | grep Ubuntu` ]; then
	PLATFORM="ubuntu"
elif [ `python -mplatform | grep centos` ]; then
	PLATFORM="centos"
else
	echo "Unknown platform"
	exit 1
fi

echo "Platform: $PLATFORM"

msbuild msbuild/build.proj /flp:LogFile=Build.log
msbuild msbuild/deploy.proj /flp:LogFile=Deploy.log /p:DeployTo=$DeployTo

if [ "$PLATFORM" = "ubuntu" ]; then
	cd install/deb
	rm -R -f ../*.deb ../*.changes Files/Services Files/WebStudio Files/sql Files/licenses
	mkdir Files/sql
	cp -R ../../../licenses Files/licenses
	cp -R ../../deploy/$DeployTo/Services Files
	cp -R ../../deploy/$DeployTo/WebStudio Files/WebStudio
	cp ../../sql/onlyoffice* Files/sql

	if [ -t 0 ]; then
		# if in terminal
		dpkg-buildpackage -b
	else
		dpkg-buildpackage -b -p"$SIGN_COMMAND"
	fi
	mv -f ../onlyoffice* ../../deploy/
else
	cd install/rpm
	rm -R -f Files/onlyoffice/Services Files/onlyoffice/WebStudio Files/onlyoffice/Sql Files/onlyoffice/Licenses
	mkdir Files/onlyoffice/Sql
	cp -R ../../../licenses Files/onlyoffice/Licenses
	cp -R ../../deploy/$DeployTo/Services Files/onlyoffice/Services
	cp -R ../../deploy/$DeployTo/WebStudio Files/onlyoffice/WebStudio
	cp ../../sql/onlyoffice* Files/onlyoffice/Sql

	sh buildhelper.sh -b -u
	mv -f builddir/RPMS/noarch/onlyoffice* ../../deploy/
fi
