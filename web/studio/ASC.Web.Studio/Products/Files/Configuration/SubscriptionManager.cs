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