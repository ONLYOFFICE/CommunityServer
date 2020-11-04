/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
