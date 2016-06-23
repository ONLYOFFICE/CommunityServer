/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Data;
using ASC.Common.Module;
using ASC.Notify.Config;
using ASC.Web.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using log4net;
using log4net.Config;
using System.ServiceModel;
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
            XmlConfigurator.Configure();

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
