
The configuration file for the aggregator consists of the following sections:
- configSections
- nlog
- appSettings
- system.data
- connectionStrings
- runtime
- startup


"configSections" section
Purpose:
	Specifies configuration sections and declares namespaces.
Description:
	in order to work correctly, WatchDog requires the nlog subsection, 
that should be configured as shown in the following code example.
Configuration example:
	<configSections>
		<section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
	</configSections>
  
  
"nlog" section
Purpose:
	configures the WatchDog logging system (see http://nlog-project.org/)
Description:
	WatchDog can log messages with the following logging levels: debug, trace, info, warn, error, fatal. By default, WatchDog logs data on its activity:
	- to the "logs/ASC.Mail.WatchDog.log.txt" file (is archived once a day), all levels except debug;
	- to the console (if WatchDog is run in the console mode), all levels except debug;
	- to the "logs\errors\{date}.errors.log.txt" file, 
if a message has the error or fatal level.
Configuration example:
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" autoReload="true" throwExceptions="true" internalLogFile="${basedir}/logs/Nlog.txt" internalLogLevel="Warn" internalLogToConsoleError="true">
		<targets>
		  <target name="file" type="AsyncWrapper" queueLimit="5000" overflowAction="Block">
			<target xsi:type="File" layout="${longdate} ${threadid:padding=3} ${level:padding=5} ${logger:padding=18} ${message} ${exception:format=tostring}" fileName="${basedir}/logs/ASC.Mail.WatchDog.log.txt" archiveFileName="${basedir}/logs/archives/log.{#}.txt" archiveEvery="Day" archiveNumbering="Rolling" maxArchiveFiles="7" concurrentWrites="true" keepFileOpen="false" encoding="iso-8859-2"/>
		  </target>
		  <target name="errors" type="AsyncWrapper" queueLimit="5000" overflowAction="Block">
			<target xsi:type="File" layout="${longdate} ${threadid:padding=3} ${level:padding=5} ${logger:padding=18} ${message} ${exception:format=tostring}" fileName="${basedir}/logs/errors/${shortdate}.errors.log.txt"/>
		  </target>
		  <target name="console" xsi:type="ColoredConsole" layout="${date:format=HH\:mm\:ss} ${threadid:padding=3} ${logger:padding=18} ${message}">
			<highlight-row backgroundColor="NoChange" condition="level == LogLevel.Info" foregroundColor="Green"/>
		  </target>
		</targets>
		<rules>
		  <logger name="*" minlevel="Trace" writeTo="file, console"/>
		  <logger name="*" minLevel="Error" writeTo="errors"/>
		</rules>
	</nlog>
  
  
  
"appSettings" section
Purpose:
	stores the parameters required for the application operation
Description:
	old_tasks_timeout_in_minutes - an interval (in minutes) between checking and discarding old tasks executed by aggregators
Configuration example:
	<appSettings>
		<add key="old_tasks_timeout_in_minutes" value="10"/>
	</appSettings>
  
  
  
"system.data" section
Purpose:
	configures a provider used to access a database
Description:
	this section configures DbManager from ASC.Common.Data.dll.
Configuration example:
	<system.data>
		<DbProviderFactories>
			<clear/>
			<add name="MySQL Data Provider" invariant="MySql.Data.MySqlClient" description=".Net Framework Data Provider for MySQL" type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data"/>
		</DbProviderFactories>
	</system.data>  
  
  
  
"connectionStrings" section
Purpose:
	configures connections to the database.
Description:
	this section configures DbManager from ASC.Common.Data.dll.
Configuration example:
	<connectionStrings>
		<clear/>
		<add name="mail" connectionString="Server=teamlab;Database=Test;UserID=dev;Pwd=dev;pooling=True;Character Set=utf8" providerName="MySql.Data.MySqlClient"/>
	</connectionStrings> 
  
  
  
"runtime" section
Purpose:
  contains information on the assembly binding
Description:
  supportedRuntime - contains information on the assembly version redirect
Configuration example:
	<runtime>
		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
		</assemblyBinding>
	</runtime>
  
  
"startup" section
Purpose:
   defines the information on the Common Language Runtime.
Variables description:
	supportedRuntime - defines the Common Language Runtime version that the application supports 
Configuration example:
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/>
  </startup>