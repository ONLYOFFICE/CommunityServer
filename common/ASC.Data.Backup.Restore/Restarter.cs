/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                RestartService("TeamlabOfficeServer");
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
            var service = ServiceController.GetServices().Where(s => s.ServiceName == serviceName).FirstOrDefault();
            if (service != null)
            {
                var controllers = new List<ServiceController>();
                controllers.AddRange(service.DependentServices);
                controllers.Add(service);
                RestartServices(controllers);
            }
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
