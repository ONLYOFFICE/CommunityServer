echo off

set basepath=%cd%
for %%D in ("%CD%") do set "parentdir=%%~nxD"
set version=
if not "%~2" == "" set version=%~2

SET parent=%~dp0
FOR %%a IN ("%parent:~0,-1%") DO SET grandparent=%%~dpa

if "%~1" == "--install" (
	sc create Onlyoffice%parentdir%%version%      start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\"
	goto Exit
)

if "%~1" == "--install-all" (
	sc create OnlyofficeNotify%version%            start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Notify.NotifyServiceLauncher, ASC.Notify\" --log Notify"
	sc create OnlyofficeJabber%version%            start= delayed-auto binPath= "\"%grandparent%\Jabber\ASC.Xmpp.Server.Launcher.exe\"" 
	sc create OnlyofficeIndex%version%             start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.ElasticSearch.Launcher, ASC.ElasticSearch\" --log Index"
	sc create OnlyOfficeStorageMigrate%version%    start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Data.Storage.Migration.Launcher,ASC.Data.Storage.Migration\" --log StorageMigrate"
	sc create OnlyOfficeStorageEncryption%version% start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Data.Storage.Encryption.Launcher,ASC.Data.Storage.Encryption\" --log StorageEncryption"
	sc create OnlyofficeFeed%version%              start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Feed.Aggregator.FeedAggregatorLauncher, ASC.Feed.Aggregator\" --log Feed"
	sc create OnlyOfficeAutoCleanUp%version%       start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Files.AutoCleanUp.Launcher, ASC.Files.AutoCleanUp\" --log AutoCleanUp"
	sc create OnlyofficeBackup%version%            start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Data.Backup.Service.BackupServiceLauncher, ASC.Data.Backup\" --log Backup"
	sc create OnlyOfficeTelegram%version%          start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.TelegramService.Launcher, ASC.TelegramService\" --log Telegram"
	sc create OnlyofficeThumbnailBuilder%version%  start= delayed-auto binPath= "\"%basepath%\TeamLabSvc.exe\" --service \"ASC.Files.ThumbnailBuilder.Launcher, ASC.Files.ThumbnailBuilder\" --log ThumbnailBuilder"
	sc create OnlyOfficeMailAggregator%version%    start= delayed-auto binPath= "\"%grandparent%\MailAggregator\ASC.Mail.Aggregator.CollectionService.exe\""
	sc create OnlyOfficeMailWatchdog%version%      start= delayed-auto binPath= "\"%grandparent%\MailWatchdog\ASC.Mail.Watchdog.Service.exe\""
	sc create OnlyOfficeMailCleaner%version%       start= delayed-auto binPath= "\"%grandparent%\MailCleaner\ASC.Mail.StorageCleaner.exe\""
	
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\Radicale.xml
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\SsoAuth.xml
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\SocketIO.xml
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\WebDav.xml
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\UrlShortener.xml
	call %basepath%\WinSW3.0.0.exe install %grandparent%\run\Thumb.xml
	
	sc failure OnlyofficeNotify%version%             reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeJabber%version%             reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeIndex%version%              reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeStorageMigrate%version%     reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeStorageEncryption%version%  reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeAutoCleanUp%version%        reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeFeed%version%               reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeBackup%version%             reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeTelegram%version%           reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyofficeThumbnailBuilder%version%   reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeMailAggregator%version%     reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeMailWatchdog%version%       reset= 60  actions= restart/60000/restart/60000/restart/60000
	sc failure OnlyOfficeMailCleaner%version%        reset= 60  actions= restart/60000/restart/60000/restart/60000
	
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
	net stop  OnlyOfficeStorageMigrate%version%
	sc delete OnlyOfficeStorageMigrate%version%
	net stop  OnlyOfficeStorageEncryption%version%
	sc delete OnlyOfficeStorageEncryption%version%
	net stop  OnlyofficeAutoCleanUp%version%	
	sc delete OnlyofficeAutoCleanUp%version%
	net stop  OnlyofficeFeed%version%
	sc delete OnlyofficeFeed%version%
	net stop  OnlyofficeBackup%version%
	sc delete OnlyofficeBackup%version%	
	net stop  OnlyOfficeTelegram%version%
	sc delete OnlyOfficeTelegram%version%
	net stop  OnlyofficeThumbnailBuilder%version%
	sc delete OnlyofficeThumbnailBuilder%version%
	net stop  OnlyOfficeMailAggregator%version%
	sc delete OnlyOfficeMailAggregator%version%	
	net stop  OnlyOfficeMailWatchdog%version%
	sc delete OnlyOfficeMailWatchdog%version%
	net stop  OnlyOfficeMailCleaner%version%
	sc delete OnlyOfficeMailCleaner%version%	
	
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\Radicale.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\Radicale.xml
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\SsoAuth.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\SsoAuth.xml
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\SocketIO.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\SocketIO.xml
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\WebDav.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\WebDav.xml
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\UrlShortener.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\UrlShortener.xml
	call %basepath%\WinSW3.0.0.exe stop %grandparent%\run\Thumb.xml
	call %basepath%\WinSW3.0.0.exe uninstall %grandparent%\run\Thumb.xml
	
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