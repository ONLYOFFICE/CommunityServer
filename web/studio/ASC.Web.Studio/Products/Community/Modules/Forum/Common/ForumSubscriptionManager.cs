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
using System.Web;
using ASC.Core;
using ASC.Forum;
using ASC.Notify.Model;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Studio.Utility;
using ASC.Web.Community.Product;
using ASC.Notify.Recipients;

namespace ASC.Web.Community.Forum
{
    public class ForumSubscriptionManager : ISubscriptionManager,
                                          ISubscriptionGetcherView
    {
        private Guid _threadSubscriptionTypeID = new Guid("{61230AB9-96A0-4da7-9F45-6DAB04A95F29}");
        private Guid _topicSubscriptionTypeID = new Guid("{59B90C57-747B-4b2b-97DC-8AD73D8B0829}");
        private Guid _topicOnForumSubscriptionTypeID = new Guid("{7730BE18-4733-427d-BC3B-6ABF82F88559}");


        private List<SubscriptionObject> GetSubscriptionObjectsByType(Guid productID, Guid moduleID, Guid typeID)
        {
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

            InitSubscriptionGetcherPresenter();
            if (GetSubscriptionObjects != null)
            {
                if (typeID.Equals(_threadSubscriptionTypeID))
                {

                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewPostInThread, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        foreach (Thread thread in this.SubscriptionObjects)
                        {
                            subscriptionObjects.Add(new SubscriptionObject()
                            {
                                ID = thread.ID.ToString(),
                                Name = thread.Title,
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/Topics.aspx") + "?f=" + thread.ID.ToString(),
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_threadSubscriptionTypeID))
                            });
                        }
                    }
                }

                else if (typeID.Equals(_topicSubscriptionTypeID))
                {
                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewPostInTopic, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        foreach (Topic topic in this.SubscriptionObjects)
                        {
                            subscriptionObjects.Add(new SubscriptionObject()
                            {
                                ID = topic.ID.ToString(),
                                Name = topic.Title,
                                URL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/Posts.aspx") + "?t=" + topic.ID.ToString(),
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_topicSubscriptionTypeID))
                            });
                        }
                    }
                }

                else if (typeID.Equals(_topicOnForumSubscriptionTypeID))
                {
                    GetSubscriptionObjects(this, new SubscriptionEventArgs(SubscriptionConstants.NewTopicInForum, SecurityContext.CurrentAccount.ID, TenantProvider.CurrentTenantID));
                    if (this.SubscriptionObjects != null && this.SubscriptionObjects.Count > 0)
                    {
                        subscriptionObjects.Add(new SubscriptionObject()
                        {
                            ID = _topicOnForumSubscriptionTypeID.ToString(),
                            Name = Resources.ForumResource.NewTopicOnForumSubscriptionTitle,
                            URL = string.Empty,
                            SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_topicOnForumSubscriptionTypeID))
                        });
                    }
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

        public ForumSubscriptionManager()
        {

        }

        private bool _isInitSubscriptionGetcher = false;
        private void InitSubscriptionGetcherPresenter()
        {
            if (_isInitSubscriptionGetcher == false)
            {
                ForumManager.Instance.PresenterFactory.GetPresenter<ISubscriptionGetcherView>().SetView(this);
                _isInitSubscriptionGetcher = true;
            }
        }

        #region ISubscriptionManager Members

        List<SubscriptionObject> ISubscriptionManager.GetSubscriptionObjects(Guid subItem)
        {
            List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();
            foreach (var type in GetSubscriptionTypes())
                subscriptionObjects.AddRange(GetSubscriptionObjectsByType(CommunityProduct.ID, ForumManager.ModuleID, type.ID));

            return subscriptionObjects;
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscriptionType>();

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _topicOnForumSubscriptionTypeID,
                Name = Resources.ForumResource.NewTopicOnForumSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewTopicInForum,
                Single = true,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                CanSubscribe = true
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _threadSubscriptionTypeID,
                Name = Resources.ForumResource.ThreadSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewPostInThread,
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = _topicSubscriptionTypeID,
                Name = Resources.ForumResource.TopicSubscriptionTitle,
                NotifyAction = SubscriptionConstants.NewPostInTopic,
                GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
            });


            return subscriptionTypes;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return SubscriptionConstants.SubscriptionProvider; }
        }

        #endregion

        #region ISubscriptionGetcherView Members

        public event EventHandler<SubscriptionEventArgs> GetSubscriptionObjects;

        public IList<object> SubscriptionObjects { get; set; }

        #endregion
    }
}
