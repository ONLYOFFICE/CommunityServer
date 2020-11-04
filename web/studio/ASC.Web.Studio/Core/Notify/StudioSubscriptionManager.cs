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
using ASC.Web.Core.Subscriptions;
using ASC.Notify.Model;

namespace ASC.Web.Studio.Core.Notify
{
    internal class StudioSubscriptionManager : ISubscriptionManager
    {
        private static StudioSubscriptionManager _instance = new StudioSubscriptionManager();

        public static StudioSubscriptionManager Instance
        {
            get { return _instance; }
        }

        private StudioSubscriptionManager()
        { }

        #region ISubscriptionManager Members

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var types = new List<SubscriptionType>();
            types.Add(new SubscriptionType()
            {
                ID = new Guid("{148B5E30-C81A-4ff8-B749-C46BAE340093}"),
                Name = Resources.Resource.WhatsNewSubscriptionName,
                NotifyAction = Actions.SendWhatsNew,
                Single = true
            });

            var astype = new SubscriptionType()
            {
                ID = new Guid("{A4FFC01F-BDB5-450e-88C4-03FED17D67C5}"),
                Name = Resources.Resource.AdministratorNotifySenderTypeName,
                NotifyAction = Actions.SendWhatsNew,
                Single = false
            };
            
            types.Add(astype);

            return types;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return StudioNotifyHelper.SubscriptionProvider; }
        }

        #endregion
    }
}
