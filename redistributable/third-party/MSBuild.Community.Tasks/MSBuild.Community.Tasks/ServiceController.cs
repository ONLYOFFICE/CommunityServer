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
    #region Enums
    /// <summary>
    /// Defines the actions that can be performed on a service.
    /// </summary>
    public enum ServiceActions
    {
        /// <summary>
        /// Starts a service.
        /// </summary>
        Start,

        /// <summary>
        /// Stops a service.
        /// </summary>
        Stop,

        /// <summary>
        /// Restarts a service.
        /// </summary>
        Restart,

        /// <summary>
        /// Pauses a running service.
        /// </summary>
        Pause,

        /// <summary>
        /// Continues a paused service.
        /// </summary>
        Continue
    }

    #endregion Enums

    /// <summary>
	/// Task that can control a Windows service.
	/// </summary>
    /// <example>
    /// <para>Restart Web Server</para>
    /// <code><![CDATA[
    /// <ServiceController ServiceName="w3svc" Action="Restart" />
    /// ]]></code>
    /// </example>
    public class ServiceController : ServiceQuery
	{

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ServiceController"/> class.
		/// </summary>
		public ServiceController()
		{

		}

		#endregion Constructor

		#region Properties
		private ServiceActions _action;

		/// <summary>
        /// Gets or sets the <see cref="T:ServiceActions"/> to perform on the service.
		/// </summary>
		/// <value>The action to perform on the service.</value>
        /// <enum cref="MSBuild.Community.Tasks.ServiceActions"/>
		[Required]
		public string Action
		{
			get { return _action.ToString(); }
			set { _action = (ServiceActions)Enum.Parse(typeof(ServiceActions), value); }
		}

		private double _timeout = TimeSpan.FromSeconds(60).TotalMilliseconds;

		/// <summary>
		/// Gets or sets the timeout for the command. The default is
		/// one minute.
		/// </summary>
		/// <value>The timeout for the command.</value>
		public double Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}
		#endregion

		#region Task overrides
		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			bool result = true;
			Service controller = null;

			try
			{
				controller = GetServiceController();
				if (controller == null)
				{
					throw new Exception(string.Format(Properties.Resources.ServiceNotFound,
						ServiceName, MachineName));
				}


				ServiceControllerStatus desiredStatus = DetermineDesiredStatus();
				ServiceControllerStatus currentStatus = controller.Status;

				if (currentStatus == desiredStatus && _action != ServiceActions.Restart)
				{
					Log.LogMessage(Properties.Resources.ServiceStatus,
						DisplayName, MachineName, currentStatus);

					return true;
				}

				switch (_action)
				{
					case ServiceActions.Start:
						result = StartService(controller);
						break;
					case ServiceActions.Pause:
						result = PauseService(controller);
						break;
					case ServiceActions.Continue:
						result = ContinueService(controller);
						break;
					case ServiceActions.Stop:
						result = StopService(controller);
						break;
					case ServiceActions.Restart:
						result = RestartService(controller);
						break;
				}

				// refresh current service status
				controller.Refresh();
				base.Status = controller.Status.ToString();

				Log.LogMessage(Properties.Resources.ServiceStatus,
					DisplayName, MachineName, Status);

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

			return result;
		}

		#endregion Task overrides

		#region Private Methods
		private ServiceControllerStatus DetermineDesiredStatus()
		{
			switch (_action)
			{
				case ServiceActions.Stop:
					return ServiceControllerStatus.Stopped;
				case ServiceActions.Pause:
					return ServiceControllerStatus.Paused;
				default:
					return ServiceControllerStatus.Running;
			}
		}

		private bool StartService(Service serviceController)
		{
			Log.LogMessage(Properties.Resources.ServiceStarting, DisplayName);

			if (serviceController.Status == ServiceControllerStatus.Paused)
			{
				serviceController.Continue();
			}
			else
			{
				serviceController.Start();
			}

			// wait until service is running or timeout expired
			serviceController.WaitForStatus(ServiceControllerStatus.Running,
				TimeSpan.FromMilliseconds(Timeout));

			Log.LogMessage(Properties.Resources.ServiceStarted, DisplayName);

			return true;
		}

		private bool StopService(Service serviceController)
		{
			if (!serviceController.CanStop)
			{
				Log.LogError(Properties.Resources.ServiceCannotStop,
					ServiceName, MachineName);
				return false;
			}

			Log.LogMessage(Properties.Resources.ServiceStopping, DisplayName);
			serviceController.Stop();

			// wait until service is stopped or timeout expired
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped,
				TimeSpan.FromMilliseconds(Timeout));

			Log.LogMessage(Properties.Resources.ServiceStopped, DisplayName);

			return true;
		}

		private bool RestartService(Service serviceController)
		{
			if (serviceController.Status != ServiceControllerStatus.Stopped)
			{
				StopService(serviceController);
			}
			return StartService(serviceController);
		}

		private bool PauseService(Service serviceController)
		{

			if (!serviceController.CanPauseAndContinue)
			{
				Log.LogError(Properties.Resources.ServiceCannotPause,
					ServiceName, MachineName);
				return false;
			}

			if (serviceController.Status != ServiceControllerStatus.Running)
			{
				Log.LogError(Properties.Resources.ServiceNotStarted,
					ServiceName, MachineName);
				return false;
			}

			Log.LogMessage(Properties.Resources.ServicePausing, DisplayName);
			serviceController.Pause();

			// wait until service is paused or timeout expired
			serviceController.WaitForStatus(ServiceControllerStatus.Paused,
				TimeSpan.FromMilliseconds(Timeout));

			Log.LogMessage(Properties.Resources.ServicePaused, DisplayName);

			return true;
		}

		private bool ContinueService(Service serviceController)
		{

			if (serviceController.Status == ServiceControllerStatus.Running)
			{
				return true; // already running
			}

			if (!serviceController.CanPauseAndContinue)
			{
				Log.LogError(Properties.Resources.ServiceCannotContinue,
					ServiceName, MachineName);
				return false;
			}
			if (serviceController.Status != ServiceControllerStatus.Paused)
			{
				Log.LogError(Properties.Resources.ServiceNotPaused,
					ServiceName, MachineName);
				return false;
			}

			Log.LogMessage(Properties.Resources.ServiceContinuing, DisplayName);

			serviceController.Continue();

			// wait until service is running or timeout expired
			serviceController.WaitForStatus(ServiceControllerStatus.Running,
				TimeSpan.FromMilliseconds(Timeout));

			Log.LogMessage(Properties.Resources.ServiceContinued, DisplayName);

			return true;
		}

		#endregion Private Methods
	}
}
