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
using System.ServiceProcess;
using Service = System.ServiceProcess.ServiceController;




namespace MSBuild.Community.Tasks
{
	/// <summary>
	/// Task that can determine the status of a specified service
	/// on a target server.
	/// </summary>
    /// <example>
    /// <para>Check status of SQL Server</para>
    /// <code><![CDATA[
    /// <ServiceQuery ServiceName="MSSQLServer">
    ///     <Output TaskParameter="Status" PropertyName="ResultStatus" />
    /// </ServiceQuery>
    /// <Message Text="MSSQLServer Service Status: $(ResultStatus)"/>
    /// ]]></code>
    /// </example>
    public class ServiceQuery : Task
	{
		#region Constants

		/// <summary>
		/// The unknown <see cref="Status"/>
		/// returned when the service does not exist.
		/// The value is "Unknown".
		/// </summary>
		public const string UNKNOWN_STATUS = @"Unknown";

		#endregion Constants

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ServiceQuery"/> class.
		/// </summary>
		public ServiceQuery()
		{

		}

		#endregion Constructor

		#region Properties
		private string _serviceName;

		/// <summary>
		/// Gets or sets the name of the service.
		/// </summary>
		/// <value>The name of the service.</value>
		[Required]
		public string ServiceName
		{
			get { return _serviceName; }
			set { _serviceName = value; }
		}

		private string _machineName;

		/// <summary>
		/// Gets or sets the name of the machine.
		/// </summary>
		/// <value>The name of the machine.</value>
		public string MachineName
		{
			get { return _machineName; }
			set { _machineName = value; }
		}

		private string _status;

		/// <summary>
		/// Gets or sets the status.
		/// </summary>
		/// <value>The status of the service.</value>
		[Output]
		public string Status
		{
			get { return _status; }
			set { _status = value; }
		}

		private bool _canPauseAndContinue;

		/// <summary>
		/// Gets a value indicating whether the service can be paused and resumed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can pause and continue; otherwise, <c>false</c>.
		/// </value>
		[Output]
		public bool CanPauseAndContinue
		{
			get { return _canPauseAndContinue; }
		}

		private bool _canShutdown;

		/// <summary>
		/// Gets a value indicating whether the service should be notified when the system is shutting down. 
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can shutdown; otherwise, <c>false</c>.
		/// </value>
		[Output]
		public bool CanShutdown
		{
			get { return _canShutdown; }
		}

		private bool _canStop;

		/// <summary>
		/// Gets a value indicating whether the service can be stopped after it has started. 
		/// </summary>
		/// <value><c>true</c> if this instance can stop; otherwise, <c>false</c>.</value>
		[Output]
		public bool CanStop
		{
			get { return _canStop; }
		}

		private string _displayName;

		/// <summary>
		/// Gets a friendly name for the service. 
		/// </summary>
		/// <value>The name of the display.</value>
		[Output]
		public string DisplayName
		{
			get { return _displayName; }
		}

		private bool _exists;

		/// <summary>
		/// Gets a value indicating whether the service exists.
		/// </summary>
		/// <value><c>true</c> if the service exists; otherwise, <c>false</c>.</value>
		[Output]
		public bool Exists
		{
			get { return _exists; }
		}

		#endregion

		#region Task Overrides
		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			Service controller = null;
			try
			{
				controller = GetServiceController();
				if (controller != null)
				{
					Log.LogMessage(Properties.Resources.ServiceStatus,
						  _displayName, _machineName, _status);
				}
				else
				{
					Log.LogMessage(Properties.Resources.ServiceNotFound,
						_serviceName, _machineName);
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
			finally
			{
				if (controller != null)
					controller.Dispose();
			}

			return true;
		}

		#endregion Task Overrides

		#region Protected Methods
		/// <summary>
		/// Gets the service controller.
		/// </summary>
		/// <returns></returns>
		protected Service GetServiceController()
		{
			if (string.IsNullOrEmpty(_machineName))
				_machineName = Environment.MachineName;

			// get handle to service            
			try
			{
				Service controller = new Service(_serviceName, _machineName);
				_status = controller.Status.ToString();
				_canPauseAndContinue = controller.CanPauseAndContinue;
				_canShutdown = controller.CanShutdown;
				_canStop = controller.CanStop;
				_displayName = controller.DisplayName;
				_exists = true;

				return controller;
			}
			catch
			{
				_status = UNKNOWN_STATUS;
				_exists = false;
				return null;
			}
		}

		#endregion Protected Methods
	}
}
