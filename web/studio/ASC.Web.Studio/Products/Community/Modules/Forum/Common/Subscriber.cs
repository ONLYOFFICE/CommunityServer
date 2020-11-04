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
using AjaxPro;
using ASC.Core;
using ASC.Forum;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Forum
{
    [AjaxNamespace("Subscriber")]
    public class Subscriber : ISubscriberView
    {
        public Subscriber()
        {
            ForumManager.Instance.PresenterFactory.GetPresenter<ISubscriberView>().SetView(this);
        }

        #region Topic

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnTopic(int idTopic, int statusNotify)
        {
            var resp = new AjaxResponse();

            if (statusNotify == 1 && Subscribe != null)
            {
                Subscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic,
                                                       idTopic.ToString(),
                                                       SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderTopicSubscription(false, idTopic);
                resp.rs3 = "subscribed";
            }
            else if (UnSubscribe != null)
            {
                UnSubscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic,
                                                         idTopic.ToString(),
                                                         SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderTopicSubscription(true, idTopic);
                resp.rs3 = "unsubscribed";
            }
            else
            {
                resp.rs1 = "0";
                resp.rs2 = Resources.ForumResource.ErrorSubscription;
            }
            return resp;
        }

        public string RenderTopicSubscription(bool isSubscribe, int topicID)
        {
            var sb = new StringBuilder();

            sb.Append("<a id=\"statusSubscribe\"  class=\"" +
                      "follow-status " + (IsTopicSubscribe(topicID) ? "subscribed" : "unsubscribed") +
                      "\" title=\"" + (!isSubscribe ? Resources.ForumResource.UnSubscribeOnTopic : Resources.ForumResource.SubscribeOnTopic) +
                      "\" href=\"#\"onclick=\"ForumSubscriber.SubscribeOnTopic('" + topicID + "', " + (isSubscribe ? 1 : 0) + "); return false;\"></a>");

            return sb.ToString();
        }

        public bool IsTopicSubscribe(int topicID)
        {
            if (GetSubscriptionState != null)
                GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInTopic,
                                                                  topicID.ToString(),
                                                                  SecurityContext.CurrentAccount.ID));

            return IsSubscribe;
        }

        #endregion

        #region Thread

        public string RenderThreadSubscription(bool isSubscribe, int threadID)
        {
            var sb = new StringBuilder();

            sb.Append("<a id=\"statusSubscribe\"  class=\"" +
                      "follow-status " + (IsThreadSubscribe(threadID) ? "subscribed" : "unsubscribed") +
                      "\" title=\"" + (!isSubscribe ? Resources.ForumResource.UnSubscribeOnThread : Resources.ForumResource.SubscribeOnThread) +
                      "\" href=\"#\" onclick=\"ForumSubscriber.SubscribeOnThread('" + threadID + "', " + (isSubscribe ? 1 : 0) + "); return false;\"></a>");

            return sb.ToString();
        }

        public bool IsThreadSubscribe(int threadID)
        {
            if (GetSubscriptionState != null)
                GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInThread,
                                                                  threadID.ToString(),
                                                                  SecurityContext.CurrentAccount.ID));

            return IsSubscribe;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnThread(int idThread, int statusNotify)
        {
            var resp = new AjaxResponse();
            if (statusNotify == 1 && Subscribe != null)
            {
                Subscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInThread,
                                                       idThread.ToString(),
                                                       SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderThreadSubscription(false, idThread);
                resp.rs3 = "subscribed";
            }
            else if (UnSubscribe != null)
            {
                UnSubscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewPostInThread,
                                                         idThread.ToString(),
                                                         SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderThreadSubscription(true, idThread);
                resp.rs3 = "unsubscribed";
            }
            else
            {
                resp.rs1 = "0";
                resp.rs2 = Resources.ForumResource.ErrorSubscription;
            }
            return resp;
        }

        #endregion

        #region Topic on forum

        public string RenderNewTopicSubscription(bool isSubscribe)
        {
            var sb = new StringBuilder();

            sb.Append("<div id=\"forum_subcribeOnNewTopicBox\">");
            sb.Append("<a class='linkAction' href=\"#\" onclick=\"ForumSubscriber.SubscribeOnNewTopics(" + (isSubscribe ? 1 : 0) + "); return false;\">" + (!isSubscribe ? Resources.ForumResource.UnsubscribeOnNewTopicInForum : Resources.ForumResource.SubscribeOnNewTopicInForum) + "</a>");
            sb.Append("</div>");

            return sb.ToString();
        }

        public bool IsNewTopicSubscribe()
        {
            if (GetSubscriptionState != null)
                GetSubscriptionState(this, new SubscribeEventArgs(SubscriptionConstants.NewTopicInForum,
                                                                  null,
                                                                  SecurityContext.CurrentAccount.ID));

            return IsSubscribe;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnNewTopic(int statusNotify)
        {
            var resp = new AjaxResponse();
            if (statusNotify == 1 && Subscribe != null)
            {
                Subscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewTopicInForum,
                                                       null,
                                                       SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderNewTopicSubscription(false);
            }
            else if (UnSubscribe != null)
            {
                UnSubscribe(this, new SubscribeEventArgs(SubscriptionConstants.NewTopicInForum,
                                                         null,
                                                         SecurityContext.CurrentAccount.ID));

                resp.rs1 = "1";
                resp.rs2 = RenderNewTopicSubscription(true);
            }
            else
            {
                resp.rs1 = "0";
                resp.rs2 = Resources.ForumResource.ErrorSubscription;
            }
            return resp;
        }

        #endregion

        #region ISubscriberView Members

        public event EventHandler<SubscribeEventArgs> GetSubscriptionState;

        public bool IsSubscribe { get; set; }

        public event EventHandler<SubscribeEventArgs> Subscribe;

        public event EventHandler<SubscribeEventArgs> UnSubscribe;

        #endregion
    }
}