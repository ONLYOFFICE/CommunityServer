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


using System;
using System.Collections.Generic;
using System.Web;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Core.Users;
using SubscrType = ASC.Web.Core.Subscriptions.SubscriptionType;

namespace ASC.Web.Community.Blogs
{
    public class BlogsSubscriptionManager : ISubscriptionManager
    {
        private Guid _blogSubscriptionTypeID = new Guid("{4954EB99-1402-46e6-80B6-8734ABE9B8C2}");
        private Guid _blogPersSubscriptionTypeID = new Guid("{8D5AAC98-076A-44be-A718-508124BCE107}");
        private Guid _commentSubscriptionTypeID = new Guid("{615508B1-5FF9-449d-B6A9-831498EE3A93}");

        private List<SubscriptionObject> GetSubscriptionObjectsByType(Guid productID, Guid moduleID, Guid typeID)
        {
            var _engine = BasePage.GetEngine();

            var subscriptionObjects = new List<SubscriptionObject>();
            var subscriptionProvider = _engine.NotifySource.GetSubscriptionProvider();

            if (typeID.Equals(_blogSubscriptionTypeID))
            {
                var list = new List<string>(
                    subscriptionProvider.GetSubscriptions(
                        Constants.NewPost,
                        _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                    );

                if (list.Contains(null))
                {
                    subscriptionObjects.Add(new SubscriptionObject
                        {
                            ID = new Guid(Constants._NewBlogSubscribeCategory).ToString(),
                            Name = ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewPostTitle,
                            URL = string.Empty,
                            SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_blogSubscriptionTypeID))
                        });
                }
            }

            else if (typeID.Equals(_blogPersSubscriptionTypeID))
            {
                var list = new List<string>(
                    subscriptionProvider.GetSubscriptions(
                        Constants.NewPostByAuthor,
                        _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                    );
                if (list.Count > 0)
                {
                    foreach (string id in list)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            subscriptionObjects.Add(new SubscriptionObject
                                {
                                    ID = id,
                                    Name = DisplayUserSettings.GetFullUserName(new Guid(id)),
                                    URL = VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/") + "?userid=" + id,
                                    SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_blogPersSubscriptionTypeID))
                                });
                        }
                    }
                }
            }

            else if (typeID.Equals(_commentSubscriptionTypeID))
            {
                var list = new List<string>(
                    subscriptionProvider.GetSubscriptions(
                        Constants.NewComment,
                        _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                    );

                if (list.Count > 0)
                {
                    IList<Post> postList = _engine.SelectPostsInfo(list.ConvertAll(s => new Guid(s)));

                    foreach (Post post in postList)
                    {
                        if (post != null)
                        {
                            subscriptionObjects.Add(new SubscriptionObject
                                {
                                    ID = post.ID.ToString(),
                                    Name = post.Title,
                                    URL = VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/viewblog.aspx") + "?blogid=" + post.ID.ToString(),
                                    SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_commentSubscriptionTypeID))
                                });
                        }
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

        private static INotifyAction GetNotifyActionBySubscriptionType(ASC.Blogs.Core.SubscriptionType subscriptionType)
        {
            switch (subscriptionType)
            {
                case ASC.Blogs.Core.SubscriptionType.NewBlog:
                    return Constants.NewPost;

                case ASC.Blogs.Core.SubscriptionType.NewBlogPers:
                    return Constants.NewPostByAuthor;

                case ASC.Blogs.Core.SubscriptionType.NewComment:
                    return Constants.NewComment;

            }
            return null;
        }

        private ASC.Blogs.Core.SubscriptionType GetBlogsSubscriptionType(Guid subscriptionTypeID)
        {
            if (subscriptionTypeID.Equals(_blogSubscriptionTypeID))
                return ASC.Blogs.Core.SubscriptionType.NewBlog;

            else if (subscriptionTypeID.Equals(_blogPersSubscriptionTypeID))
                return ASC.Blogs.Core.SubscriptionType.NewBlogPers;

            else if (subscriptionTypeID.Equals(_commentSubscriptionTypeID))
                return ASC.Blogs.Core.SubscriptionType.NewComment;

            return ASC.Blogs.Core.SubscriptionType.NewBlog;
        }

        #region ISubscriptionManager Members

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            var _engine = BasePage.GetEngine();

            var subscriptionObjects = new List<SubscriptionObject>();
            var subscriptionProvider = _engine.NotifySource.GetSubscriptionProvider();

            #region new blogs

            var list = new List<string>(
                subscriptionProvider.GetSubscriptions(
                    Constants.NewPost,
                    _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                );

            if (list.Contains(null))
            {
                subscriptionObjects.Add(new SubscriptionObject
                    {
                        ID = new Guid(Constants._NewBlogSubscribeCategory).ToString(),
                        Name = ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewPostTitle,
                        URL = string.Empty,
                        SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_blogSubscriptionTypeID))
                    });
            }

            #endregion

            #region personal posts

            list = new List<string>(
                subscriptionProvider.GetSubscriptions(
                    Constants.NewPostByAuthor,
                    _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                );
            if (list.Count > 0)
            {
                foreach (string id in list)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        subscriptionObjects.Add(new SubscriptionObject
                            {
                                ID = id,
                                Name = DisplayUserSettings.GetFullUserName(new Guid(id)),
                                URL = VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/") + "?userid=" + id,
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_blogPersSubscriptionTypeID))
                            });
                    }
                }
            }

            #endregion

            #region new comments

            list = new List<string>(
                subscriptionProvider.GetSubscriptions(
                    Constants.NewComment,
                    _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()), false)
                );

            if (list.Count > 0)
            {
                IList<Post> postList = _engine.SelectPostsInfo(list.ConvertAll(s => new Guid(s)));

                foreach (var post in postList)
                {
                    if (post != null)
                    {
                        subscriptionObjects.Add(new SubscriptionObject
                            {
                                ID = post.ID.ToString(),
                                Name = post.Title,
                                URL = VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/viewblog.aspx") + "?blogid=" + post.ID.ToString(),
                                SubscriptionType = GetSubscriptionTypes().Find(st => st.ID.Equals(_commentSubscriptionTypeID))
                            });
                    }
                }
            }

            #endregion

            return subscriptionObjects;
        }

        public List<Web.Core.Subscriptions.SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscrType>
                {
                    new SubscrType
                        {
                            ID = _blogSubscriptionTypeID,
                            Name = ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewPostTitle,
                            NotifyAction = Constants.NewPost,
                            Single = true,
                            IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                            CanSubscribe = true
                        },
                    new SubscrType
                        {
                            ID = _blogPersSubscriptionTypeID,
                            Name = ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnAuthorTitle,
                            NotifyAction = Constants.NewPostByAuthor,
                            GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                            IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
                        },
                    new SubscrType
                        {
                            ID = _commentSubscriptionTypeID,
                            Name = ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewCommentsTitle,
                            NotifyAction = Constants.NewComment,
                            GetSubscriptionObjects = new GetSubscriptionObjectsDelegate(GetSubscriptionObjectsByType),
                            IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType)
                        }
                };

            return subscriptionTypes;
        }

        public void UnsubscribeForObject(string subscriptionObjectID, Guid subscriptionTypeID)
        {
            var _engine = BasePage.GetEngine();

            var subscriptionProvider = _engine.NotifySource.GetSubscriptionProvider();

            subscriptionProvider.UnSubscribe(
                GetNotifyActionBySubscriptionType(
                    GetBlogsSubscriptionType(subscriptionTypeID)),
                (subscriptionTypeID == _blogSubscriptionTypeID ? null : subscriptionObjectID),
                _engine.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString())
                );
        }

        public void UnsubscribeForType(Guid subscriptionTypeID)
        {
        }

        #endregion

        #region ISubscriptionManager Members


        public ISubscriptionProvider SubscriptionProvider
        {
            get { return BasePage.GetEngine().NotifySource.GetSubscriptionProvider(); }
        }

        #endregion
    }
}