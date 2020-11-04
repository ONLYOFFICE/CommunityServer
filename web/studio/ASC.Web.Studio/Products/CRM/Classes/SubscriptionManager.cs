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


#region Import

using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Notify.Model;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Core.Utility.Skins;

#endregion

namespace ASC.Web.CRM
{

    public class ProductSubscriptionManager : IProductSubscriptionManager
    {

        private readonly Guid _setAccess = new Guid("{D4D58C55-D32E-41dc-9D22-D123AFAFC7E7}");
        private readonly Guid _responsibleForTask = new Guid("{2479B115-EAEB-4d9a-86DA-51BD708DEFDC}");
        private readonly Guid _responsibleForOpprotunity = new Guid("{73720A31-1981-480f-AB34-074E8BAEDBA9}");
        private readonly Guid _addRelationshipEvent = new Guid("{4E16DBC5-A427-469e-9EF7-A8DEA0F61310}");
        private readonly Guid _exportCompleted = new Guid("{88D3DC5E-3E46-46a1-9FEF-6B8FFF020BA4}");
        private readonly Guid _importCompleted = new Guid("{6A717AAD-16AE-4713-A782-B887766BEB9F}");
        private readonly Guid _createNewContact = new Guid("{ADAC1E70-4163-41c1-8968-67A44E4D24E7}");
  
        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        { 
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            return new List<SubscriptionType>
                       {
                           new SubscriptionType
                           {
                               ID = _setAccess,
                               Name = CRMCommonResource.SubscriptionType_SetAccess,
                               NotifyAction = NotifyConstants.Event_SetAccess,
                               Single = true,
                               CanSubscribe = true
                           },
                           new SubscriptionType
                               {
                                   ID = _responsibleForTask,
                                   Name = CRMCommonResource.SubscriptionType_ResponsibleForTask,
                                   NotifyAction = NotifyConstants.Event_ResponsibleForTask,
                                   Single = true,
                                   CanSubscribe = true
                            },
                           new SubscriptionType
                               {
                                   ID = _responsibleForOpprotunity,
                                   Name = CRMCommonResource.SubscriptionType_ResponsibleForOpportunity,
                                   NotifyAction = NotifyConstants.Event_ResponsibleForOpportunity,
                                   Single = true,
                                   CanSubscribe = true
                           },
                           new SubscriptionType
                           {
                               ID = _addRelationshipEvent,
                               Name = CRMCommonResource.SubscriptionType_AddRelationshipEvent,
                               NotifyAction = NotifyConstants.Event_AddRelationshipEvent,
                               Single = true,
                               CanSubscribe = true
                           },
                            new SubscriptionType
                           {
                               ID = _exportCompleted,
                               Name = CRMCommonResource.SubscriptionType_ExportCompleted,
                               NotifyAction = CoreContext.Configuration.CustomMode ? NotifyConstants.Event_ExportCompletedCustomMode : NotifyConstants.Event_ExportCompleted,
                               Single = true,
                               CanSubscribe = true
                           },
                            new SubscriptionType
                           {
                               ID = _importCompleted,
                               Name = CRMCommonResource.SubscriptionType_ImportCompleted,
                               NotifyAction = CoreContext.Configuration.CustomMode ? NotifyConstants.Event_ImportCompletedCustomMode : NotifyConstants.Event_ImportCompleted,
                               Single = true,
                               CanSubscribe = true
                           },
                            new SubscriptionType
                           {
                               ID = _createNewContact,
                               Name = CRMCommonResource.SubscriptionType_CreateNewContact,
                               NotifyAction = NotifyConstants.Event_CreateNewContact,
                               Single = true,
                               CanSubscribe = true
                           }
                       };
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return NotifySource.Instance.GetSubscriptionProvider(); }
        }

        public GroupByType GroupByType
        {
            get { return GroupByType.Simple; }
        }

        public List<SubscriptionGroup> GetSubscriptionGroups()
        {
            return  new List<SubscriptionGroup>();
        }
    }

}