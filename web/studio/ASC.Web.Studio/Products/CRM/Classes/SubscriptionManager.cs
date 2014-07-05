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

#region Import

using System;
using System.Collections.Generic;
using System.Web;
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
                               NotifyAction = NotifyConstants.Event_ExportCompleted,
                               Single = true,
                               CanSubscribe = true
                           },
                            new SubscriptionType
                           {
                               ID = _importCompleted,
                               Name = CRMCommonResource.SubscriptionType_ImportCompleted,
                               NotifyAction = NotifyConstants.Event_ImportCompleted,
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