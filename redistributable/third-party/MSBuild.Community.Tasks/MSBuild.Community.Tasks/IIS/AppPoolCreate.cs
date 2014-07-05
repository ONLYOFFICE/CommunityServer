#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion



using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.DirectoryServices;

namespace MSBuild.Community.Tasks.IIS
{
	/// <summary>
	/// Creates a new application pool on a local or remote machine with IIS installed.  The default is 
	/// to create the new application pool on the local machine.  If connecting to a remote machine, you can
	/// specify the <see cref="WebBase.Username"/> and <see cref="WebBase.Password"/> for the task
	/// to run under.
	/// </summary>
	/// <example>Create a new application pool on the local machine.
	/// <code><![CDATA[
	/// <AppPoolCreate AppPoolName="MyAppPool" />
	/// ]]></code>
	/// </example>
    public class AppPoolCreate : WebBase
	{
		#region Fields

		private string mAppPoolName;
		private bool mAppPoolAutoStart = true;
		private int mAppPoolIdentityType = 2;
		private int mAppPoolQueueLength = 1000;
		private string mAutoShutdownAppPoolExe = "";
		private string mAutoShutdownAppPoolParams = "";
		private int mCPUAction = -1;
		private int mCPULimit = 0;
		private int mCPUResetInterval = 5;
		private bool mDisallowOverlappingRotation = false;
		private bool mDisallowRotationOnConfigChange = false;
		private int mIdleTimeout = 20;
		private int mLoadBalancerCapabilities = 2;
		private int mLogEventOnRecycle = 137;
		private int mLogonMethod = -1;
		private int mMaxProcesses = 1;
		private string mOrphanActionExe = "";
		private string mOrphanActionParams = "";
		private bool mOrphanWorkerProcess = false;
		private int mPeriodicRestartMemory = 0;
		private int mPeriodicRestartPrivateMemory = 0;
		private int mPeriodicRestartRequests = 0;
		private string[] mRestartSchedule;
		private string mPeriodicRestartSchedule = null;
		private int mPeriodicRestartTime = 1740;
		private bool mPingingEnabled = true;
		private int mPingInterval = 30;
		private int mPingResponseTime = 90;
		private bool mRapidFailProtection = true;
		private int mRapidFailProtectionInterval = 5;
		private int mRapidFailProtectionMaxCrashes = 5;
		private int mShutdownTimeLimit = 90;
		private bool mSMPAffinitized = false;
		private long mSMPProcessorAffinityMask = 4294967295;
		private int mStartupTimeLimit = 90;
		private string mWAMUserName = "";
		private string mWAMUserPass = "";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the application pool.
		/// </summary>
		/// <value>The name of the application pool.</value>
		[Required]
		public string ApplicationPoolName
		{
			get
			{
				return mAppPoolName;
			}
			set
			{
				mAppPoolName = value;
			}
		}

		/// <summary>
		/// The AppPoolAutoStart property indicates to the World Wide Web Publishing Service (WWW service)
		/// to automatically start an application pool when the application pool is created or when IIS
		/// is started, if the value of this property is set to true.
		/// </summary>
		/// <value>Value indicating if AppPoolAutoStart is enabled or disabled.</value>
		public bool AppPoolAutoStart
		{
			get
			{
				return mAppPoolAutoStart;
			}
			set
			{
				mAppPoolAutoStart = value;
			}
		}

		/// <summary>
		/// The AppPoolIdentityType property allows application pools to run as a specific user account:
		/// 
		/// 0 - The application pool runs as NT AUTHORITY\SYSTEM.
		/// 1 - The application pool runs as NT AUTHORITY\LOCAL SERVICE.
		/// 2 - The application pool runs as NT AUTHORITY\NETWORK SERVICE.
		/// 3 - The application pool runs as a specific user account, defined by the <see cref="WAMUserName"/> property.
		/// </summary>
		/// <value>Value indicating the application pool identity type.</value>
		public int AppPoolIdentityType
		{
			get
			{
				return mAppPoolIdentityType;
			}
			set
			{
				mAppPoolIdentityType = value;
			}
		}

		/// <summary>
		/// The AppPoolQueueLength property indicates to the Universal Listener how many requests
		/// to queue up for an application pool before rejecting future requests. When the limit
		/// for this property is exceeded, IIS rejects the additional requests with a 503 error.
		/// </summary>
		/// <value>Value indicating the application pool queue length.</value>
		public int AppPoolQueueLength
		{
			get
			{
				return mAppPoolQueueLength;
			}
			set
			{
				mAppPoolQueueLength = value;
			}
		}

		/// <summary>
		/// The AutoShutdownAppPoolExe property specifies an executable to run when the World Wide Web
		/// Publishing Service (WWW service) shuts down an application pool for rapid fail protection. You
		/// can use the <see cref="AutoShutdownAppPoolParams"/> property to send parameters to the executable.
		/// </summary>
		/// <value>Value indicating the application pool auto shutdown executable.</value>
		public string AutoShutdownAppPoolExe
		{
			get
			{
				return mAutoShutdownAppPoolExe;
			}
			set
			{
				mAutoShutdownAppPoolExe = value;
			}
		}

		/// <summary>
		/// The AutoShutdownAppPoolParams property specifies any command-line parameters for the executable that
		/// is specified in the AutoShutdownAppPoolExe property. You can use these two properties in the following
		/// way to send e-mail, for example, when the World Wide Web Publishing Service (WWW service) shuts down
		/// an application pool for rapid fail protection:
		/// 
		/// AutoShutdownAppPoolExe = "C:\LogAndSendMail.bat"
		/// AutoShutdownAppPoolParams = "-AppPoolName %1%"
		/// 
		/// where %1% represents the application pool name.
		/// </summary>
		/// <value>Value indicating the parameters for the application pool auto shutdown executable.</value>
		public string AutoShutdownAppPoolParams
		{
			get
			{
				return mAutoShutdownAppPoolParams;
			}
			set
			{
				mAutoShutdownAppPoolParams = value;
			}
		}

		/// <summary>
		/// The CPUAction property configures the action(s) that IIS takes when Microsoft Windows NT ® job objects
		/// run. Only one Windows NT job object exists per application pool, therefore the CPUAction property
		/// is configured on a per application pool basis.
		/// 
		/// Possible values:
		/// 0 - No action is taken except that a warning is written to the event log when the CPU limit is exceeded.
		/// 1 - Application pool worker processes that exceed their CPU limit will be forced to shut down.
		/// </summary>
		/// <value>Value indicating the CPU action.</value>
		public int CPUAction
		{
			get
			{
				return mCPUAction;
			}
			set
			{
				mCPUAction = value;
			}
		}

		/// <summary>
		/// The CPULimit property configures the maximum percentage of CPU resources that worker processes
		/// in an application pool are allowed to consume over a period of time, as indicated by the
		/// <see cref="CPUResetInterval"/> property. Set this property by specifying a percentage of CPU
		/// usage, multiplied by 1000. For example, if you want the CPU usage limit to be 50%, set CPULimit to 50,000.
		/// </summary>
		/// <value>Value indicating the CPU limit.</value>
		public int CPULimit
		{
			get
			{
				return mCPULimit;
			}
			set
			{
				mCPULimit = value;
			}
		}

		/// <summary>
		/// The CPUResetInterval property specifies the reset period (in minutes) for CPU monitoring and
		/// throttling limits on the application pool. When the number of minutes elapsed since the last
		/// process accounting reset equals the number specified by this property, IIS will reset the CPU
		/// timers for both the logging and limit intervals. Setting the value of this property to 0
		/// disables CPU monitoring.
		/// </summary>
		/// <value>Value indicating the CPU reset interval.</value>
		public int CPUResetInterval
		{
			get
			{
				return mCPUResetInterval;
			}
			set
			{
				mCPUResetInterval = value;
			}
		}

		/// <summary>
		/// The DisallowOverlappingRotation property specifies whether or not the World Wide Web Publishing
		/// Service (WWW Service) should start up another worker process to replace the existing worker
		/// process while it is shutting down.
		/// </summary>
		/// <value>Value indicating the DisallowOverlappingRotation.</value>
		public bool DisallowOverlappingRotation
		{
			get
			{
				return mDisallowOverlappingRotation;
			}
			set
			{
				mDisallowOverlappingRotation = value;
			}
		}

		/// <summary>
		/// The DisallowRotationOnConfigChange property specifies whether or not the World Wide Web Publishing
		/// Service (WWW Service) should rotate worker processes in an application pool when the configuration
		/// has changed. This means that the worker processes will not pick up application pool changes to
		/// values passed to the worker process, such as <see cref="IdleTimeout"/> and <see cref="PeriodicRestartRequests"/>.
		/// </summary>
		/// <value>Value indicating the DisallowRotationOnConfigChange.</value>
		public bool DisallowRotationOnConfigChange
		{
			get
			{
				return mDisallowRotationOnConfigChange;
			}
			set
			{
				mDisallowRotationOnConfigChange = value;
			}
		}

		/// <summary>
		/// The IdleTimeout property specifies how long (in minutes) a worker process should run idle if no new
		/// requests are received and the worker process is not processing requests. After the allotted time
		/// passes, the worker process should request to be shut down by the World Wide Web Publishing Service (WWW Service).
		/// </summary>
		/// <value>Value indicating the idle timeout.</value>
		public int IdleTimeout
		{
			get
			{
				return mIdleTimeout;
			}
			set
			{
				mIdleTimeout = value;
			}
		}

		/// <summary>
		/// The LoadBalancerCapabilities property specifies behavior when a service is unavailable. A setting of 1
		/// terminates the connection. A setting of 2 sends error code 503.
		/// </summary>
		/// <value>Value indicating the load balancer capabilities.</value>
		public int LoadBalancerCapabilities
		{
			get
			{
				return mLoadBalancerCapabilities;
			}
			set
			{
				mLoadBalancerCapabilities = value;
			}
		}

		/// <summary>
		/// The LogEventOnRecycle property specifies that IIS should log an event when an application pool is
		/// recycled. Application pools recycle for a variety of reasons. In order for IIS to log the event, the
		/// LogEventOnRecycle property must have a bit set corresponding to the reason for the recycle.
		/// </summary>
		/// <value>Value indicating which recycle events to log.</value>
		public int LogEventOnRecycle
		{
			get
			{
				return mLogEventOnRecycle;
			}
			set
			{
				mLogEventOnRecycle = value;
			}
		}

		/// <summary>
		/// The LogonMethod property contains an integer that specifies the logon method for cleartext 
		/// logons. Valid settings are:
		/// 
		/// 0 for interactive logon.
		/// 1 for batch logon.
		/// 2 for network logon.
		/// 3 for cleartext logon.
		/// </summary>
		/// <value>Value indicating the logon method.</value>
		public int LogonMethod
		{
			get
			{
				return mLogonMethod;
			}
			set
			{
				mLogonMethod = value;
			}
		}

		/// <summary>
		/// The MaxProcesses property determines the maximum number of worker processes an application pool
		/// allows to service requests for an application pool. This property cannot be set to 0 because there
		/// are no unmanaged pools.
		/// </summary>
		/// <value>Value indicating the maximum number of worker processes allowed by the application pool.</value>
		public int MaxProcesses
		{
			get
			{
				return mMaxProcesses;
			}
			set
			{
				mMaxProcesses = value;
			}
		}

		/// <summary>
		/// The OrphanActionExe property specifies an executable to run when the World Wide Web Publishing
		/// Service (WWW service) orphans a worker process. You can use the <see cref="OrphanActionParams"/> property
		/// to send parameters to the executable.
		/// </summary>
		/// <value>The value for the orphan action executable.</value>
		public string OrphanActionExe
		{
			get
			{
				return mOrphanActionExe;
			}
			set
			{
				mOrphanActionExe = value;
			}
		}

		/// <summary>
		/// The OrphanActionParams property specifies command-line parameters for the executable
		/// specified by the <see cref="OrphanActionExe"/> property.
		/// </summary>
		/// <value>Value indicating the orphan action parameters.</value>
		public string OrphanActionParams
		{
			get
			{
				return mOrphanActionParams;
			}
			set
			{
				mOrphanActionParams = value;
			}
		}

		/// <summary>
		/// The OrphanWorkerProcess property, when set to true, notifies the World Wide Web Publishing
		/// Service (WWW Service) not to terminate a worker process that fails to respond to pings, but
		/// to instead orphan the worker process in the application pool if the worker process suffers
		/// fatal errors.
		/// </summary>
		/// <value>Value indicating the orphan worker process.</value>
		public bool OrphanWorkerProcess
		{
			get
			{
				return mOrphanWorkerProcess;
			}
			set
			{
				mOrphanWorkerProcess = value;
			}
		}

		/// <summary>
		/// The PeriodicRestartMemory property specifies the amount of virtual memory (in KB) that a
		/// worker process can use before the worker process recycles. The maximum value supported
		/// for this property is 4,294,967 KB.
		/// </summary>
		/// <value>Value indicating the amount of memory.</value>
		public int PeriodicRestartMemory
		{
			get
			{
				return mPeriodicRestartMemory;
			}
			set
			{
				mPeriodicRestartMemory = value;
			}
		}

		/// <summary>
		/// The PeriodicRestartPrivateMemory property specifies the amount of private memory (in KB) that a
		/// worker process can use before the worker process recycles. The maximum value supported
		/// for this property is 4,294,967 KB.
		/// </summary>
		/// <value>Value indicating the amount of memory.</value>
		public int PeriodicRestartPrivateMemory
		{
			get
			{
				return mPeriodicRestartPrivateMemory;
			}
			set
			{
				mPeriodicRestartPrivateMemory = value;
			}
		}

		/// <summary>
		/// The PeriodicRestartRequests property indicates the number of requests the OOP application
		/// should process, after which it is recycled.
		/// </summary>
		/// <value>Value indicating the number of requests.</value>
		public int PeriodicRestartRequests
		{
			get
			{
				return mPeriodicRestartRequests;
			}
			set
			{
				mPeriodicRestartRequests = value;
			}
		}

		/// <summary>
		/// The PeriodicRestartSchedule property specifies the time (in 24 hour format) that the application
		/// will be rotated. Each time is in local time and is specified in the following format:
		/// 
		/// PeriodicRestartSchedule="hh:mm,hh:mm,hh:mm"
		/// </summary>
		/// <value>Value indicating the restart schedule.</value>
		public string PeriodicRestartSchedule
		{
			get
			{
				return mPeriodicRestartSchedule;
			}
			set
			{
				if (value.Length > 0)
				{
					mRestartSchedule = value.Split(',');
					for (int iCtr = 0; iCtr < mRestartSchedule.Length; iCtr++)
					{
						mRestartSchedule[iCtr] = mRestartSchedule[iCtr].Trim();
					}
				}

				mPeriodicRestartSchedule = value;
			}
		}

		/// <summary>
		/// The PeriodicRestartTime property specifies the period of time, in minutes, after which IIS
		/// rotates an isolated OOP application. Setting the value of this property to 0 disables the
		/// property. The maximum supported value for this property is 71,582.
		/// </summary>
		/// <value>Value indicating the restart time period.</value>
		public int PeriodicRestartTime
		{
			get
			{
				return mPeriodicRestartTime;
			}
			set
			{
				mPeriodicRestartTime = value;
			}
		}

		/// <summary>
		/// The PingingEnabled property specifies whether the World Wide Web Publishing Service
		/// (WWW Service) should periodically monitor the health of a worker process. Setting the
		/// value of this property to true indicates to the WWW service to monitor the worker
		/// processes to ensure that the they are running and healthy.
		/// </summary>
		/// <value>Value indicating if pinging is enabled or disabled.</value>
		public bool PingingEnabled
		{
			get
			{
				return mPingingEnabled;
			}
			set
			{
				mPingingEnabled = value;
			}
		}

		/// <summary>
		/// The PingInterval property specifies the period of time (in seconds) between health-monitoring
		/// pings that the World Wide Web Publishing Service (WWW Service) sends to a worker process.
		/// </summary>
		/// <value>Value indicating the ping interval.</value>
		public int PingInterval
		{
			get
			{
				return mPingInterval;
			}
			set
			{
				mPingInterval = value;
			}
		}

		/// <summary>
		/// The PingResponseTime property specifies the amount of time (in seconds) that a worker process
		/// is given to respond to a health monitoring ping. After the time limit is exceeded, the World
		/// Wide Web Publishing Service (WWW Service) terminates the worker process.
		/// </summary>
		/// <value>Value indicating the ping response time.</value>
		public int PingResponseTime
		{
			get
			{
				return mPingResponseTime;
			}
			set
			{
				mPingResponseTime = value;
			}
		}

		/// <summary>
		/// Setting the RapidFailProtection property to true instructs the World Wide Web Publishing
		/// Service (WWW service) to put all applications in an application pool out of service if the
		/// number of worker process crashes has reached the maximum specified by the
		/// <see cref="RapidFailProtectionMaxCrashes"/> property, within the number of minutes specified
		/// by the <see cref="RapidFailProtectionInterval"/> property.
		/// </summary>
		/// <value>Value indicating if rapid fail protection is enabled or disabled.</value>
		public bool RapidFailProtection
		{
			get
			{
				return mRapidFailProtection;
			}
			set
			{
				mRapidFailProtection = value;
			}
		}

		/// <summary>
		/// The RapidFailProtectionInterval property specifies the number of minutes before the failure
		/// count for a process is reset. See <see cref="RapidFailProtection"/>.
		/// </summary>
		/// <value>Value indicating the rapid fail protection interval.</value>
		public int RapidFailProtectionInterval
		{
			get
			{
				return mRapidFailProtectionInterval;
			}
			set
			{
				mRapidFailProtectionInterval = value;
			}
		}

		/// <summary>
		/// The RapidFailProtectionMaxCrashes property specifies the maximum number of failures
		/// allowed within the number of minutes specified by the <see cref="RapidFailProtectionInterval"/> 
		/// property. See <see cref="RapidFailProtection"/>.
		/// </summary>
		/// <value>Value indicating the maximum number of crashes.</value>
		public int RapidFailProtectionMaxCrashes
		{
			get
			{
				return mRapidFailProtectionMaxCrashes;
			}
			set
			{
				mRapidFailProtectionMaxCrashes = value;
			}
		}

		/// <summary>
		/// The ShutdownTimeLimit property specifies the amount of time (in seconds) after a recycle
		/// threshold has been reached that IIS waits for all old requests to finish running in a worker
		/// process before terminating the worker process.
		/// </summary>
		/// <value>Value indicating the shutdown time limit.</value>
		public int ShutdownTimeLimit
		{
			get
			{
				return mShutdownTimeLimit;
			}
			set
			{
				mShutdownTimeLimit = value;
			}
		}

		/// <summary>
		/// Setting the SMPAffinitized property to true indicates that a particular worker process
		/// assigned to an application pool should be assigned to a given CPU. This property is used
		/// in conjunction with the <see cref="SMPProcessorAffinityMask"/> property to configure a
		/// particular processor a worker process will be assigned to.
		/// </summary>
		/// <value>Value indicating if SMPAffinitized is enabled or disabled.</value>
		public bool SMPAffinitized
		{
			get
			{
				return mSMPAffinitized;
			}
			set
			{
				mSMPAffinitized = value;
			}
		}

		/// <summary>
		/// The SMPProcessorAffinityMask property configures the hexadecimal processor mask. The hexadecimal
		/// processor mask indicates to which CPU the worker processes in an application pool should be
		/// bound. Before this property takes affect, the <see cref="SMPAffinitized"/> property must be set
		/// to true for the application pool. These properties cannot be set through IIS Manager.
		/// 
		/// Do not set this property to zero. Doing so causes no SMP affinity to be configured, creating an
		/// error condition. The default DWORD value is 4294967295 (or -1), which is represented in hexadecimal
		/// as 0xFFFFFFFF. A value of 0xFFFFFFFF in SMPProcessorAffinityMask indicates that all processors are enabled.
		/// </summary>
		/// <value>Value indicating the SMP processor affinity bit mask.</value>
		public long SMPProcessorAffinityMask
		{
			get
			{
				return mSMPProcessorAffinityMask;
			}
			set
			{
				mSMPProcessorAffinityMask = value;
			}
		}

		/// <summary>
		/// The value of the StartupTimeLimit property specifies the amount of time (in seconds) that the World Wide
		/// Web Publishing Service (WWW Service) should wait for a worker process to finish starting up and
		/// reporting to the WWW Service.
		/// </summary>
		/// <value>Value indicating the startup time limit.</value>
		public int StartupTimeLimit
		{
			get
			{
				return mStartupTimeLimit;
			}
			set
			{
				mStartupTimeLimit = value;
			}
		}

		/// <summary>
		/// The WAMUserName property specifies the account user name that IIS uses by default as the COM+
		/// application identity for newly created IIS out-of-process applications. The values of this
		/// property and its companion property, <see cref="WAMUserPass"/>, are set when IIS is installed, and
		/// match the user name and password values in the Microsoft Windows user account, which is established
		/// at the same time. Changing the value of this property is not recommended. If you do, change
		/// it to a valid Windows user account, and change WAMUserPass to the corresponding password
		/// for the new account.
		/// 
		/// Important:
		/// Changes to WAMUserName and WAMUserPass may disrupt the operation of existing IIS out-of-process
		/// applications. You can synchronize application identities using Component Services to edit the
		/// user name and password values, found on the Identity tab of the property sheet for each package.
		/// 
		/// In-process applications are not affected by these property values.
		/// </summary>
		/// <value>Value indicating the username.</value>
		public string WAMUserName
		{
			get
			{
				return mWAMUserName;
			}
			set
			{
				mWAMUserName = value;
			}
		}

		/// <summary>
		/// The WAMUserPass property specifies the password for the account that IIS uses by default as the COM+
		/// application identity for newly created IIS out-of-process applications. The values of this property
		/// and its companion property, <see cref="WAMUserName"/>, are set when IIS is installed, and match the
		/// password and user name values in the Microsoft Windows user account (IWAM_ MachineName, where MachineName
		/// is the name of the machine on which IIS is installed) established at the same time.
		/// 
		/// Important:
		/// Changing the value of this property is not recommended. If you do, you must change the Windows account
		/// password to the identical value. You must also synchronize existing IIS out-of-process application
		/// identities, using Component Services to edit the user name and password values, which are found on the
		/// Identity tab of the property sheet for each package.
		/// 
		/// In-process applications are not affected by these property values.
		/// </summary>
		/// <value>Value indicating the password.</value>
		public string WAMUserPass
		{
			get
			{
				return mWAMUserPass;
			}
			set
			{
				mWAMUserPass = value;
			}
		}

		#endregion

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// True if the task successfully executed; otherwise, false.
		/// </returns>
        public override bool Execute()
        {
			mIISVersion = GetIISVersion();

			if (CreateAppPool())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#region Private Methods

		private bool CreateAppPool()
		{
			bool bSuccess = false;
			Log.LogMessage(MessageImportance.Normal, "Creating application pool named {0}/{1}:", IISAppPoolPath, ApplicationPoolName);

			try
			{
				VerifyIISRoot();

				if (mIISVersion == IISVersion.Six)
				{
					DirectoryEntry appPools = new DirectoryEntry(IISAppPoolPath);
					appPools.RefreshCache();
					DirectoryEntry newPool;

					try
					{
						// Determine if the application pool already exists
						DirectoryEntry existingPool = appPools.Children.Find(ApplicationPoolName, "IIsApplicationPool");
						newPool = existingPool;
					}
					catch
					{
						// Create the application pool
						newPool = appPools.Children.Add(ApplicationPoolName, "IIsApplicationPool");
						newPool.CommitChanges();
					}

					// Set the AppPool Properties
					newPool.Properties["AppPoolAutoStart"].Value = AppPoolAutoStart;
					newPool.Properties["AppPoolIdentityType"].Value = AppPoolIdentityType;
					newPool.Properties["AppPoolQueueLength"].Value = AppPoolQueueLength;
					if (AutoShutdownAppPoolExe.Length > 0)
					{
						newPool.Properties["AutoShutdownAppPoolExe"].Value = AutoShutdownAppPoolExe;
					}
					if (AutoShutdownAppPoolParams.Length > 0)
					{
						newPool.Properties["AutoShutdownAppPoolParams"].Value = AutoShutdownAppPoolParams;
					}
					if (CPUAction >= 0)
					{
						newPool.Properties["CPUAction"].Value = CPUAction;
					}
					newPool.Properties["CPULimit"].Value = CPULimit;
					newPool.Properties["CPUResetInterval"].Value = CPUResetInterval;
					newPool.Properties["DisallowOverlappingRotation"].Value = DisallowOverlappingRotation;
					newPool.Properties["DisallowRotationOnConfigChange"].Value = DisallowRotationOnConfigChange;
					newPool.Properties["IdleTimeout"].Value = IdleTimeout;
					newPool.Properties["LoadBalancerCapabilities"].Value = LoadBalancerCapabilities;
					newPool.Properties["LogEventOnRecycle"].Value = LogEventOnRecycle;
					if (LogonMethod >= 0)
					{
						newPool.Properties["LogonMethod"].Value = LogonMethod;
					}
					newPool.Properties["MaxProcesses"].Value = MaxProcesses;
					if (OrphanActionExe.Length > 0)
					{
						newPool.Properties["OrphanActionExe"].Value = OrphanActionExe;
					}
					if (OrphanActionParams.Length > 0)
					{
						newPool.Properties["OrphanActionParams"].Value = OrphanActionParams;
					}
					newPool.Properties["OrphanWorkerProcess"].Value = OrphanWorkerProcess;
					newPool.Properties["PeriodicRestartMemory"].Value = PeriodicRestartMemory;
					newPool.Properties["PeriodicRestartPrivateMemory"].Value = PeriodicRestartPrivateMemory;
					newPool.Properties["PeriodicRestartRequests"].Value = PeriodicRestartRequests;
					if (PeriodicRestartSchedule.Length > 0)
					{
						newPool.Properties["PeriodicRestartSchedule"].Value = mRestartSchedule;
					}
					newPool.Properties["PeriodicRestartTime"].Value = PeriodicRestartTime;
					newPool.Properties["PingingEnabled"].Value = PingingEnabled;
					newPool.Properties["PingInterval"].Value = PingInterval;
					newPool.Properties["PingResponseTime"].Value = PingResponseTime;
					newPool.Properties["RapidFailProtection"].Value = RapidFailProtection;
					newPool.Properties["RapidFailProtectionInterval"].Value = RapidFailProtectionInterval;
					newPool.Properties["RapidFailProtectionMaxCrashes"].Value = RapidFailProtectionMaxCrashes;
					newPool.Properties["ShutdownTimeLimit"].Value = ShutdownTimeLimit;
					newPool.Properties["SMPAffinitized"].Value = SMPAffinitized;
					newPool.Properties["SMPProcessorAffinityMask"].Value = SMPProcessorAffinityMask;
					newPool.Properties["StartupTimeLimit"].Value = StartupTimeLimit;
					
					if (WAMUserName.Length > 0 && WAMUserPass.Length > 0)
					{
						newPool.Properties["WAMUserName"].Value = WAMUserName;
						newPool.Properties["WAMUserPass"].Value = WAMUserPass;
					}
					
					// Commit the changes
					newPool.CommitChanges();
					appPools.CommitChanges();
					newPool.Close();
					appPools.Close();

					bSuccess = true;
					Log.LogMessage(MessageImportance.Normal, "Done.");
				}
				else
				{
					Log.LogError("Application Pools are only available in IIS 6.");
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return bSuccess;
		}

		#endregion
	}
}
