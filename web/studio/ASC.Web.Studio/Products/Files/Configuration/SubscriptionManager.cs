/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Core;
using ASC.Notify.Model;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;

namespace ASC.Web.Files.Classes
{
    public class SubscriptionManager : IProductSubscriptionManager
    {
        private readonly Guid _subscrTypeDocuSignComplete = new Guid("{0182E476-D63D-46ED-B928-104861507811}");
        private readonly Guid _subscrTypeDocuSignStatus = new Guid("{ED7F93CD-7575-40EB-86EB-82FBA23171D2}");
        private readonly Guid _subscrTypeShareDoc = new Guid("{552846EC-AC94-4408-AAC6-17C8989B8B38}");
        private readonly Guid _subscrTypeShareFolder = new Guid("{0292A4F4-0687-42a6-9CE4-E21215045ABE}");
        private readonly Guid _subscrTypeMailMerge = new Guid("{FB5858EC-046C-41E2-84C9-B44BF7884514}");
        private readonly Guid _subscrTypeEditorMentions = new Guid("{9D3CAB90-5718-4E82-959F-27EC83BFBC5F}");

        public GroupByType GroupByType
        {
            get { return GroupByType.Simple; }
        }

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscriptionType>
                                    {
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeShareDoc,
                                                Name = FilesCommonResource.SubscriptForAccess,
                                                NotifyAction = NotifyConstants.Event_ShareDocument,
                                                Single = true,
                                                CanSubscribe = true
                                            },
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeShareFolder,
                                                Name = FilesCommonResource.ShareFolder,
                                                NotifyAction = NotifyConstants.Event_ShareFolder,
                                                Single = true,
                                                CanSubscribe = true
                                            },
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeMailMerge,
                                                Name = FilesCommonResource.SubscriptForMailMerge,
                                                NotifyAction = NotifyConstants.Event_MailMergeEnd,
                                                Single = true,
                                                CanSubscribe = true
                                            },
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeEditorMentions,
                                                Name = FilesCommonResource.EditorMentions,
                                                NotifyAction = NotifyConstants.Event_EditorMentions,
                                                Single = true,
                                                CanSubscribe = true
                                            },
                                    };

            if (CoreContext.Configuration.CustomMode) return subscriptionTypes;

            subscriptionTypes.AddRange(new List<SubscriptionType>
                                    {
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeDocuSignComplete,
                                                Name = FilesCommonResource.SubscriptDocuSignComplete,
                                                NotifyAction = NotifyConstants.Event_DocuSignComplete,
                                                Single = true,
                                                CanSubscribe = true
                                            },
                                        new SubscriptionType
                                            {
                                                ID = _subscrTypeDocuSignStatus,
                                                Name = FilesCommonResource.SubscriptDocuSignStatus,
                                                NotifyAction = NotifyConstants.Event_DocuSignStatus,
                                                Single = true,
                                                CanSubscribe = true
                                            }
                                    });

            return subscriptionTypes;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return NotifySource.Instance.GetSubscriptionProvider(); }
        }

        public List<SubscriptionGroup> GetSubscriptionGroups()
        {
            return new List<SubscriptionGroup>();
        }
    }
}