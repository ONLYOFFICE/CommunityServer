The configuration file for the aggregator consists of the following sections:
	- configSections
	- nlog
	- mail
	- system.data
	- connectionStrings
	- appSettings
	- storage
	- runtime
	- startup

"configSections" section
	Purpose:
		Specifies configuration sections and declares namespaces.
	Description:
		 in order to work correctly, the aggregator requires three subsections: nlog, storage, mail, that should be configured as shown in the following code example. 
	Configuration example:
		<configSections>
			<section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
			<section name="storage" restartOnExternalChanges="true" type="ASC.Data.Storage.Configuration.StorageConfigurationSection, ASC.Data.Storage"/>
			<section name="mail" restartOnExternalChanges="true" type="ASC.Mail.Aggregator.Client.Configuration.MailQueueConfigurationSection, ASC.Mail.Aggregator.CollectionService"/>
		</configSections>

"nlog" section
	Purpose:
		configures the aggregator logging system (see http://nlog-project.org/)
	Description:
		the aggregator can log messages with the following logging levels: debug, trace, info, warn, error, fatal. By default, the aggregator logs data on its activity:
			- to the "logs\ASC.Mail.Aggregator.log.txt" file (is archived once a day), all levels except debug.
			- to the console (if the aggregator is run in console mode), all levels except debug.
			- to the "logs\errors\{date}.errors.log.txt" file, if a message has the error or fatal level.
	Configuration example:
		<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" autoReload="true" throwExceptions="true" internalLogFile="${basedir}/logs/Nlog.txt" internalLogLevel="Warn" internalLogToConsoleError="true">
			<targets>
				<target name="file" type="AsyncWrapper" queueLimit="5000" overflowAction="Block">
				<target xsi:type="File" layout="${longdate} ${threadid:padding=3} ${level:padding=5} ${logger:padding=18} ${message} ${exception:format=tostring}" fileName="${basedir}/logs/ASC.Mail.Aggregator.log.txt" archiveFileName="${basedir}/logs/archives/log.{#}.txt" archiveEvery="Day" archiveNumbering="Rolling" maxArchiveFiles="7" concurrentWrites="true" keepFileOpen="false" encoding="iso-8859-2"/>
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

"mail" section
	Purpose:
		configures certain threads of the aggregator (configures multithreading).
	General settings:
		- check_interval="1" 		- frequency of the base queries for new tasks for a single thread in seconds.
		- activity_timeout="90" 	- time to check user activity on the portal in seconds. If the time of the last request for new messages made by a user is less than the activity_timeout value, that means that the user is active, i.e. his tasks will have the highest priority for the aggregator.
		- threads="1" 				- a number of worker threads that the aggregator starts.
		- maxnew="25" 				- a limit of new messages in the mailbox that can be processed by a single worker thread during one connection (when the limit is exceeded, the worker thread stops processing the mailbox and switches over to another task).
		- overdue_delay="600"  		- mailbox blocked time (in seconds) if payment for the portal is not made for a month.
		- longdead_delay="3600" 	- mailbox blocked time (in seconds) if payment for the portal is not made for more than a month.
	Configuration example:
		<mail>
			<queue check_interval="1" activity_timeout="90" threads="1" maxnew="25" overdue_delay="600" longdead_delay="3600" />
		</mail>

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
		configures connections to databases.
	Description:
		this section configures DbManager from ASC.Common.Data.dll.
	Configuration example:
		<connectionStrings>
			<clear/>
			<add name="mail" connectionString="Server=teamlab;Database=Test;UserID=dev;Pwd=dev;pooling=True;Character Set=utf8" providerName="MySql.Data.MySqlClient"/>
			<add name="core" connectionString="Server=teamlab;Database=Test;UserID=dev;Pwd=dev;pooling=True;Character Set=utf8" providerName="MySql.Data.MySqlClient"/>
			<add name="crm"  connectionString="Server=teamlab;Database=Test;UserID=dev;Pwd=dev;pooling=True;Character Set=utf8" providerName="MySql.Data.MySqlClient"/>
		</connectionStrings>

"appSettings" section
	Purpose:
		configures certain assemblies for the aggregator.
	General settings:
		- "core.machinekey" 		- a salt for the password encryption in ASC.Core.Common.
		- "core.virtual-dir" 		- a virtual directory setup in ASC.Core.Common.
		- "core.host" 			- a host setting for debugging GetApiRequestUrl on a local machine.
		- "core.port" 			- a port setting for debugging GetApiRequestUrl on a local machine.
		- "api.url" 			- a relative path to api.
		- "core.base-domain" 		- a domain for api requests.
		- "googleClientId" 	- an identifier to access GMail via OAuth.
		- "googleClientSecret"     - a secret key to access GMail via OAuth.
		- "mail.SendTcpTimeout" 	- a send timeout for TcpClient in ActiveUp.
		- "mail.RecieveTcpTimeout" 	- a recieve timeout for TcpClient in ActiveUp.
		- "mail.OneUserMode" 		- parameters to download mail for a particular user only (the user id should be found in the database table)
	Configuration example:
		<appSettings>
			<add key="core.machinekey" value="1234567890"/>
			<add key="core.virtual-dir" value=""/>
			<add key="core.host" value="localhost"/>
			<add key="core.port" value="80"/>
			<add key="api.url" value="~/api/2.0/"/>
			<add key="core.base-domain" value=""/>
			<add key="mail.googleClientID" value=""/>
			<add key="mail.googleClientSecret" value=""/>
			<add key="mail.SendTcpTimeout" value="30000"/>
			<add key="mail.RecieveTcpTimeout" value="30000"/>
			<add key="mail.OneUserMode" value=""/>
		</appSettings>

"storage" section
	Purpose:
		configures a mail files storage system
	Description:
		The Storage section supports 2 storage types: type="s3" (storing files in the Amazon cloud storage) and type="disc" (storing files on a local drive on the server)
		The acesskey, secretaccesskey and bucket properties are intended to access s3 (see http://aws.amazon.com/developers/access-keys/).
	Configuration example:
		<storage>
			<appender>
				<add name="generic" append="~/"/>
			</appender>
			<handler>
				<add name="disc" type="ASC.Data.Storage.DiscStorage.DiscDataStore, ASC.Data.Storage"/>
				<add name="s3" type="ASC.Data.Storage.S3.S3Storage, ASC.Data.Storage">
				<property name="acesskey" value="123456789"/>
				<property name="secretaccesskey" value="123456789"/>
				<property name="bucket" value="test-bucket"/>
				</add>
			</handler>
			<module>
				<add name="mailaggregator" data="666ceac1-4532-4f8c-9cba-8f510eca2fd1" type="s3" path="..\mail\aggregator\{0}" virtualpath="addons/mail/aggregator/{0}"/>
			</module>
		</storage>

"runtime" section
	Purpose:
		contains information on the assembly binding
	Configuration example:
		<runtime>
			<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
			  <dependentAssembly>
				<assemblyIdentity name="DotNetOpenAuth" publicKeyToken="2780CCD10D57B246" culture="neutral"/>
				<bindingRedirect oldVersion="0.0.0.0-4.1.0.0" newVersion="4.1.0.0"/>
			  </dependentAssembly>
			</assemblyBinding>
		  </runtime>

"startup" section
	Purpose:
		defines the information on the Common Language Runtime.
	Description:
		supportedRuntime - defines the Common Language Runtime version that the application supports
	Configuration example:
		  <startup>
			<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/>
		  </startup>