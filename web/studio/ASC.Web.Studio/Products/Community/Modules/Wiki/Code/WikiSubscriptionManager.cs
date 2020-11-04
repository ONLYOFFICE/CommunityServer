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
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core.Subscriptions;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki
{
    public class WikiNotifyClient
    {
        public static INotifyClient NotifyClient { get; private set; }

        static WikiNotifyClient()
        {
            NotifyClient = WorkContext.NotifyContext.NotifyService.RegisterClient(WikiNotifySource.Instance);
        }

        public static void SendNoticeAsync(string AuthorID, INotifyAction action, string objectID, SendNoticeCallback sendCallback, params ITagValue[] args)
        {
            InitiatorInterceptor initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(AuthorID, ""));
            try
            {
                NotifyClient.AddInterceptor(initatorInterceptor);
                NotifyClient.SendNoticeAsync(action, objectID, sendCallback, args);
            }
            finally
            {
                NotifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }
    }



    public class WikiSubscriptionManager : ISubscriptionManager
    {        
        private Guid _wikiSubscriptionTypeNewPageID = new Guid("{5A3F7831-D970-4e53-8BDE-C3CA990553C1}");
        private Guid _wikiSubscriptionTypeChangePageID = new Guid("{1B8408F6-88CC-416b-93B6-ADEE8AECB389}");
        private Guid _wikiSubscriptionTypeAddPageToCat = new Guid("{AE362537-3746-4231-A0A8-1A85FD79E2A9}");

        private List<SubscriptionObject> GetSubscriptionObjectsByType(Guid productID, Guid moduleID, Guid typeID)
        {
           
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();
            ISubscriptionProvider subscriptionProvider = SubscriptionProvider;

            if (typeID.Equals(_wikiSubscriptionTypeNewPageID))
            {
                List<string> wikiList = new List<string>(
                                subscriptionProvider.GetSubscriptions(
                                    Constants.NewPage,
                                    WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                                    false)
                                );

                if (wikiList.Contains(null))
                {
                    subscriptionObjects.Add(new SubscriptionObject()
                    {
                        ID = new Guid("{56A0EC10-5A1C-45ab-95AA-8F56827A8CCC}").ToString(),
                        Name = WikiResource.NotifyAction_NewPage,
                        URL = string.Empty,
                        SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeNewPageID))
                    });
                }
            }
            else if (typeID.Equals(_wikiSubscriptionTypeChangePageID))
            {
                List<string> wikiList = new List<string>(
                              subscriptionProvider.GetSubscriptions(
                                  Constants.EditPage,
                                  WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                                  false)
                              );

                foreach (string wikiObj in wikiList)
                {

                    subscriptionObjects.Add(new SubscriptionObject()
                    {
                        ID = wikiObj,
                        Name = string.IsNullOrEmpty(wikiObj) ? WikiResource.MainWikiCaption : wikiObj,
                        URL = ActionHelper.GetViewPagePath(WikiNotifySource.Instance.GetDefPageHref(), wikiObj),
                        SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeChangePageID)),
                    });
                }
            }
            else if (typeID.Equals(_wikiSubscriptionTypeAddPageToCat))
            {
                List<string> wikiList = new List<string>(
                             subscriptionProvider.GetSubscriptions(
                                 Constants.AddPageToCat,
                                 WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                                  false)
                             );



                foreach (string wikiObj in wikiList)
                {

                    subscriptionObjects.Add(new SubscriptionObject()
                    {
                        ID = wikiObj,
                        Name = wikiObj.Equals(string.Empty) ? WikiResource.MainWikiCaption : wikiObj,
                        URL = ActionHelper.GetViewPagePath(WikiNotifySource.Instance.GetDefPageHref(), wikiObj),
                        SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeAddPageToCat)),
                    });
                }
            }

            return subscriptionObjects;
        }
        private bool IsEmptySubscriptionType(Guid productID, Guid moduleID, Guid typeID)
        {
            var type = GetSubscriptionTypes().Find(t => t.ID.Equals(typeID));

            var objIDs = SubscriptionProvider.GetSubscriptions(type.NotifyAction, new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""), false);
            if (objIDs != null && objIDs.Length > 0)
                return false;

            return true;
        }
       
        #region ISubscriptionManager Members

        public ISubscriptionProvider SubscriptionProvider
        {
            get
            {
                return WikiNotifySource.Instance.GetSubscriptionProvider();
            }
        }

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
           
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

            ISubscriptionProvider subscriptionProvider = SubscriptionProvider;
            List<string> wikiList = new List<string>(
                subscriptionProvider.GetSubscriptions(
                    Constants.NewPage,
                    WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                    false)
                );

            if (wikiList.Contains(null))
            {
                subscriptionObjects.Add(new SubscriptionObject()
                {
                    ID = new Guid("{56A0EC10-5A1C-45ab-95AA-8F56827A8CCC}").ToString(),
                    Name = WikiResource.NotifyAction_NewPage,
                    URL = string.Empty,
                    SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeNewPageID))
                });
            }

            wikiList = new List<string>(
               subscriptionProvider.GetSubscriptions(
                   Constants.EditPage,
                   WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                   false)
               );



            foreach (string wikiObj in wikiList)
            {

                subscriptionObjects.Add(new SubscriptionObject()
                {
                    ID = wikiObj,
                    Name = wikiObj.Equals(string.Empty) ? WikiResource.MainWikiCaption : wikiObj,
                    URL = ActionHelper.GetViewPagePath(WikiNotifySource.Instance.GetDefPageHref(), wikiObj),
                    SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeChangePageID)),
                });
            }


            wikiList = new List<string>(
              subscriptionProvider.GetSubscriptions(
                  Constants.AddPageToCat,
                  WikiNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()),
                 false)
              );



            foreach (string wikiObj in wikiList)
            {

                subscriptionObjects.Add(new SubscriptionObject()
                {
                    ID = wikiObj,
                    Name = wikiObj.Equals(string.Empty) ? WikiResource.MainWikiCaption : wikiObj,
                    URL = ActionHelper.GetViewPagePath(WikiNotifySource.Instance.GetDefPageHref(), wikiObj),
                    SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_wikiSubscriptionTypeAddPageToCat)),
                });
            }

            return subscriptionObjects;

        }


        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscriptionType>();

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _wikiSubscriptionTypeNewPageID,
                Name = WikiResource.NotifyAction_NewPage,
                NotifyAction = Constants.NewPage,
                Single = true,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                CanSubscribe = true
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _wikiSubscriptionTypeChangePageID,
                Name = WikiResource.NotifyAction_ChangePage,
                NotifyAction = Constants.EditPage,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType)
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _wikiSubscriptionTypeAddPageToCat,
                Name = WikiResource.NotifyAction_AddPageToCat,
                NotifyAction = Constants.AddPageToCat,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType)
            });

            return subscriptionTypes;
        }



        #endregion

    }
}

