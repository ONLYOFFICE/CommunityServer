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


using System.ServiceModel;

using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Notify.Config;
using ASC.Web.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using TMResourceData;

namespace ASC.Notify
{
    public class NotifyServiceLauncher : IServiceController
    {
        private ServiceHost serviceHost;
        private NotifySender sender;
        private NotifyCleaner cleaner;


        public void Start()
        {
            serviceHost = new ServiceHost(typeof(NotifyService));
            serviceHost.Open();
            
            sender = new NotifySender();
            sender.StartSending();

            if (0 < NotifyServiceCfg.Schedulers.Count)
            {
                InitializeNotifySchedulers();
            }

            cleaner = new NotifyCleaner();
            cleaner.Start();
        }

        public void Stop()
        {
            if (sender != null)
            {
                sender.StopSending();
            }
            if (serviceHost != null)
            {
                serviceHost.Close();
            }
            if (cleaner != null)
            {
                cleaner.Stop();
            }
        }

        private void InitializeNotifySchedulers()
        {
            CommonLinkUtility.Initialize(NotifyServiceCfg.ServerRoot);
            DbRegistry.Configure();
            InitializeDbResources();
            NotifyConfiguration.Configure();
            WebItemManager.Instance.LoadItems();
            foreach (var pair in NotifyServiceCfg.Schedulers)
            {
                LogManager.GetLogger("ASC.Notify").DebugFormat("Start scheduler {0} ({1})", pair.Key, pair.Value);
                pair.Value.Invoke(null, null);
            }
        }

        private void InitializeDbResources()
        {
            DBResourceManager.PatchAssemblies();
        }
    }
}
