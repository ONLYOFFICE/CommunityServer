/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Xml.Linq;

namespace ASC.Data.Backup.Restore
{
    class Restarter : IBackupProvider
    {
        public string Name
        {
            get { return "restarter"; }
        }

        public IEnumerable<XElement> GetElements(int tenant, string[] configs, IDataWriteOperator writer)
        {
            return null;
        }

        public void LoadFrom(IEnumerable<XElement> elements, int tenant, string[] configs, IDataReadOperator reader)
        {
            try
            {
                RestartService("OnlyOffice");
                KillProcesses("w3wp");
                KillProcesses("aspnet_wp");
            }
            catch { }
            finally
            {
                OnProgressChanged("OK", 100);
            }
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;


        private void OnProgressChanged(string status, int progress)
        {
            try
            {
                if (ProgressChanged != null) ProgressChanged(this, new ProgressChangedEventArgs(status, progress));
            }
            catch { }
        }


        private void RestartService(string serviceName)
        {
            var services = ServiceController.GetServices().Where(s => s.ServiceName.StartsWith(serviceName, StringComparison.InvariantCultureIgnoreCase));
            var controllers = new List<ServiceController>();
            foreach (var service in services)
            {
                controllers.AddRange(service.DependentServices);
                controllers.Add(service);
            }
            RestartServices(controllers);
        }

        private void RestartServices(IEnumerable<ServiceController> controllers)
        {
            var timeout = TimeSpan.FromSeconds(30);
            var count = controllers.Count() * 2 + 1;
            var counter = 0;

            foreach (var controller in controllers)
            {
                try
                {
                    OnProgressChanged("Stopping service " + controller.DisplayName, counter++ * 100 / count);
                    WaitPendingStatuses(controller, timeout);
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    }
                }
                catch (Exception) { }
            }

            foreach (var controller in controllers.Reverse())
            {
                try
                {
                    OnProgressChanged("Starting service " + controller.DisplayName, counter++ * 100 / count);
                    WaitPendingStatuses(controller, timeout);
                    if (controller.Status == ServiceControllerStatus.Stopped)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    }
                }
                catch (Exception) { }
            }
        }

        private void WaitPendingStatuses(ServiceController controller, TimeSpan timeout)
        {
            if (controller.Status == ServiceControllerStatus.StartPending)
            {
                controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            if (controller.Status == ServiceControllerStatus.PausePending)
            {
                controller.WaitForStatus(ServiceControllerStatus.Paused, timeout);
            }
            if (controller.Status == ServiceControllerStatus.ContinuePending)
            {
                controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            if (controller.Status == ServiceControllerStatus.StopPending)
            {
                controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
        }

        private void KillProcesses(string processName)
        {
            OnProgressChanged("Restart process " + processName, 95);
            foreach (var process in Process.GetProcessesByName(processName))
            {
                process.Kill();
            }
        }
    }
}
