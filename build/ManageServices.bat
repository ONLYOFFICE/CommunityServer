echo off

set basepath=%cd%
for %%D in ("%CD%") do set "parentdir=%%~nxD"
set version=
if not "%~2" == "" set version=%~2

SET parent=%~dp0
FOR %%a IN ("%parent:~0,-1%") DO SET grandparent=%%~dpa

if "%~1" == "--install" (
	sc create Onlyoffice%parentdir%%version%      start= auto binPath= "\"%basepath%\TeamLabSvc.exe\"
	goto Exit
)

if "%~1" == "--install-all" (
	sc create OnlyofficeNotify%version%           start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Notify.NotifyServiceLauncher, ASC.Notify\" --log Notify"
	sc create OnlyofficeJabber%version%           start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Xmpp.Host.XmppServerLauncher, ASC.Xmpp.Host\" --log Jabber"
	sc create OnlyofficeIndex%version%            start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.FullTextIndex.Service.FullTextIndexLauncher, ASC.FullTextIndex\" --log Index"
	sc create OnlyofficeFeed%version%             start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Feed.Aggregator.FeedAggregatorLauncher, ASC.Feed.Aggregator\" --log Feed"
	sc create OnlyofficeBackup%version%           start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Data.Backup.Service.BackupServiceLauncher, ASC.Data.Backup\" --log Backup"
	sc create OnlyOfficeSocketIO%version%         start= auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Socket.IO.Svc.Launcher, ASC.Socket.IO.Svc\" --log SocketIO"
	sc create OnlyOfficeMailAggregator%version%   start= auto binPath= "\"%grandparent%\MailAggregator\ASC.Mail.Aggregator.CollectionService.exe\""
	sc create OnlyOfficeMailWatchdog%version%     start= auto binPath= "\"%grandparent%\MailWatchdog\ASC.Mail.Watchdog.Service.exe\""
	
	goto Exit
)
if "%~1" == "--uninstall" (
	net stop  Onlyoffice%parentdir%%version%
	sc delete Onlyoffice%parentdir%%version%
	goto Exit
)

if "%~1" == "--uninstall-all" (
	net stop  OnlyofficeNotify%version%
	sc delete OnlyofficeNotify%version%
	net stop  OnlyofficeJabber%version%
	sc delete OnlyofficeJabber%version%
	net stop  OnlyofficeIndex%version%
	sc delete OnlyofficeIndex%version%
	net stop  OnlyofficeFeed%version%
	sc delete OnlyofficeFeed%version%
	net stop  OnlyofficeBackup%version%
	sc delete OnlyofficeBackup%version%	
	net stop  OnlyOfficeSocketIO%version%
	sc delete OnlyOfficeSocketIO%version%	
	net stop  OnlyOfficeMailAggregator%version%
	sc delete OnlyOfficeMailAggregator%version%	
	net stop  OnlyOfficeMailWatchdog%version%
	sc delete OnlyOfficeMailWatchdog%version%	
	goto Exit
)

:Help
echo Usage: ManageServices.bat COMMAND [VERSION]
echo        COMMAND        install or unistall services:
echo                       --install         - install service
echo                       --uninstall       - uninstall service
echo                       --install-all     - install all services from this folder
echo                       --uninstall-all   - uninstall all services from this folder
echo        VERSION        service version, example: 8.5.1, can be empty

:Exit
echo on