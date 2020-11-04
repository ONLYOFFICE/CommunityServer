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
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Blogs.Core.Resources;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Studio.Utility;
using ASC.Web.Community.Resources;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("Subscriber")]
    public class Subscriber
    {
        private ISubscriptionProvider _subscriptionProvider;
        private IRecipientProvider _recipientProvider;

        public IDirectRecipient IAmAsRecipient
        {
            get { return (IDirectRecipient) _recipientProvider.GetRecipient(SecurityContext.CurrentAccount.ID.ToString()); }
        }


        public Subscriber()
        {
            var engine = BasePage.GetEngine();
            _subscriptionProvider = engine.NotifySource.GetSubscriptionProvider();
            _recipientProvider = engine.NotifySource.GetRecipientsProvider();
        }

        #region Comments

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnComments(Guid postID, int statusNotify)
        {
            var resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                        ASC.Blogs.Core.Constants.NewComment,
                        postID.ToString(),
                        IAmAsRecipient
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderCommentsSubscription(false, postID);
                }
                else
                {

                    _subscriptionProvider.UnSubscribe(
                        ASC.Blogs.Core.Constants.NewComment,
                        postID.ToString(),
                        IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderCommentsSubscription(true, postID);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnBlogComments(Guid postID, int statusNotify)
        {
            var resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                        ASC.Blogs.Core.Constants.NewComment,
                        postID.ToString(),
                        IAmAsRecipient
                        );


                    resp.rs1 = "0";
                    resp.rs2 = RenderCommentsSubscriptionLink(false, postID);
                    resp.rs3 = String.Format("({0})", CommunityResource.Subscribed.ToLower());
                }
                else
                {

                    _subscriptionProvider.UnSubscribe(
                        ASC.Blogs.Core.Constants.NewComment,
                        postID.ToString(),
                        IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderCommentsSubscriptionLink(true, postID);
                    resp.rs3 = "";
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        public string RenderCommentsSubscription(bool isSubscribe, Guid postID)
        {
            var sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnCommentsBox\">");

            sb.AppendFormat("<a class='linkAction' title='{3}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnComments('{0}', {1}); return false;\">{2}</a>",
                            postID,
                            (isSubscribe ? 1 : 0),
                            (!isSubscribe ? BlogsResource.UnSubscribeOnNewCommentsAction : BlogsResource.SubscribeOnNewCommentsAction),
                            BlogsResource.SubscribeOnNewCommentsDescription);

            sb.Append("</div>");
            return sb.ToString();
        }

        public string RenderCommentsSubscriptionLink(bool isSubscribe, Guid postID)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("<a id=\"statusSubscribe\" class=\"" + (!isSubscribe ? "subscribed" : "unsubscribed") + " follow-status\" title=\"{2}\" href=\"#\" onclick=\"BlogSubscriber.SubscribeOnBlogComments('{0}', {1}, this); return false;\"></a>",
                            postID,
                            (isSubscribe ? 1 : 0),
                            (!isSubscribe ? BlogsResource.UnSubscribeOnNewCommentsAction : BlogsResource.SubscribeOnNewCommentsAction)
                );

            return sb.ToString();
        }

        public bool IsCommentsSubscribe(Guid postID)
        {
            return
                _subscriptionProvider.IsSubscribed(
                    ASC.Blogs.Core.Constants.NewComment,
                    IAmAsRecipient,
                    postID.ToString());
        }


        #endregion

        #region PersonalBlog

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnPersonalBlog(Guid userID, int statusNotify)
        {
            var resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                        ASC.Blogs.Core.Constants.NewPostByAuthor,
                        userID.ToString(),
                        IAmAsRecipient
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscription(false, userID);
                }
                else
                {
                    _subscriptionProvider.UnSubscribe(
                        ASC.Blogs.Core.Constants.NewPostByAuthor,
                        userID.ToString(),
                        IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscription(true, userID);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribePersonalBlog(Guid userID, int statusNotify)
        {
            var resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                        ASC.Blogs.Core.Constants.NewPostByAuthor,
                        userID.ToString(),
                        IAmAsRecipient
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscriptionLink(false, userID);
                }
                else
                {
                    _subscriptionProvider.UnSubscribe(
                        ASC.Blogs.Core.Constants.NewPostByAuthor,
                        userID.ToString(),
                        IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscriptionLink(true, userID);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        public string RenderPersonalBlogSubscription(bool isSubscribe, Guid userID)
        {
            var sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnPersonalBlogBox\">");
            sb.AppendFormat("<a class='linkAction' title='{3}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnPersonalBlog('{0}', {1}); return false;\">{2}</a>",
                            userID,
                            (isSubscribe ? 1 : 0),
                            (!isSubscribe ? BlogsResource.UnSubscribeOnAuthorAction : BlogsResource.SubscribeOnAuthorAction),
                            BlogsResource.SubscribeOnAuthorDescription);
            sb.Append("</div>");

            return sb.ToString();
        }

        public string RenderPersonalBlogSubscriptionLink(bool isSubscribe, Guid userID)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("<li><a class='dropdown-item' title='{3}' href=\"#\" onclick=\"BlogSubscriber.SubscribePersonalBlog('{0}', {1}, this); return false;\">{2}</a></li>",
                            userID,
                            (isSubscribe ? 1 : 0),
                            (!isSubscribe ? BlogsResource.UnSubscribeOnAuthorAction : BlogsResource.SubscribeOnAuthorAction),
                            BlogsResource.SubscribeOnAuthorDescription);

            return sb.ToString();
        }

        public bool IsPersonalBlogSubscribe(Guid userID)
        {
            return
                _subscriptionProvider.IsSubscribed(
                    ASC.Blogs.Core.Constants.NewPostByAuthor,
                    IAmAsRecipient,
                    userID.ToString());
        }


        #endregion

        #region NewPosts

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnNewPosts(int statusNotify)
        {
            var resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                        ASC.Blogs.Core.Constants.NewPost,
                        null,
                        IAmAsRecipient
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderNewPostsSubscription(false);
                }
                else
                {
                    _subscriptionProvider.UnSubscribe(
                        ASC.Blogs.Core.Constants.NewPost,
                        null,
                        IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderNewPostsSubscription(true);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }


        public string RenderNewPostsSubscription(bool isSubscribe)
        {
            var sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnNewPostsBox\">");
            sb.AppendFormat("<a class='linkAction' title='{2}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnNewPosts({0}); return false;\">{1}</a>",
                            (isSubscribe ? 1 : 0),
                            (!isSubscribe ? BlogsResource.UnSubscribeOnNewPostAction : BlogsResource.SubscribeOnNewPostAction),
                            BlogsResource.SubscribeOnNewPostDescription);
            sb.Append("</div>");

            return sb.ToString();
        }

        public bool IsNewPostsSubscribe()
        {
            return
                _subscriptionProvider.IsSubscribed(
                    ASC.Blogs.Core.Constants.NewPost,
                    IAmAsRecipient,
                    null);
        }

        #endregion
    }
}