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
using ASC.Forum;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;
using ASC.Web.Community.Resources;

namespace ASC.Web.Community.Forum
{
    public partial class Posts : MainPage
    {
        protected string ForumPageTitle { get; set; }
        protected string ForumPageParentTitle { get; set; }
        protected string ForumPageParentURL { get; set; }
        protected string ForumPageParentIn { get; set; }

        private UserControls.Forum.Common.ForumManager _forumManager;
        public Topic Topic { get; set; }
        public Guid SettingsID { get; set; }
        protected string SubscribeStatus { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _forumManager = ForumManager.Settings.ForumManager;

            ForumManager.Instance.SetCurrentPage(ForumPage.PostList);

            var idTopic = 0;
            if (!String.IsNullOrEmpty(Request["t"]))
            {
                try
                {
                    idTopic = Convert.ToInt32(Request["t"]);
                }
                catch
                {
                    idTopic = 0;
                }
            }
            if (idTopic == 0)
                Response.Redirect("Default.aspx");


            var topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, idTopic);
            if (topic == null)
                Response.Redirect("Default.aspx");

            Topic = topic;

            var postListControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/PostListControl.ascx") as PostListControl;
            postListControl.SettingsID = ForumManager.Settings.ID;
            postListControl.Topic = topic;
            postListControl.PaggingHolder = PagingHolder;
            postListHolder.Controls.Add(postListControl);
            Utility.RegisterTypeForAjax(typeof (TopicControl), Page);
            Utility.RegisterTypeForAjax(typeof (Subscriber));
            var subscriber = new Subscriber();

            var isTopicSubscribe = subscriber.IsTopicSubscribe(topic.ID);
            var SubscribeTopicLink = subscriber.RenderTopicSubscription(!isTopicSubscribe, topic.ID);

            //master.ActionsPlaceHolder.Controls.Add(new HtmlMenuItem(subscriber.RenderThreadSubscription(!isThreadSubscribe, topic.ThreadID)));
            //master.ActionsPlaceHolder.Controls.Add(new HtmlMenuItem(subscriber.RenderTopicSubscription(!isTopicSubscribe, topic.ID)));

            ForumPageParentTitle = topic.ThreadTitle;
            ForumPageParentIn = CommunityResource.InForParentPage;
            ForumPageParentURL = "topics.aspx?f=" + topic.ThreadID.ToString();
            ForumPageTitle = topic.Title;
            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption ?? Resources.ForumResource.AddonName);
            SubscribeStatus = isTopicSubscribe ? "subscribed" : "unsubscribed";

            RenderModeratorFunctionsHeader();

            SubscribeLinkBlock.Text = SubscribeTopicLink;
        }

        protected void RenderModeratorFunctionsHeader()
        {
            var sb = new StringBuilder();
            var bitMask = 0;

            if (!Topic.IsApproved && _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Topic))
                bitMask += 1;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.TopicDelete, Topic))
                bitMask += 2;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.TopicSticky, Topic))
                bitMask += 4;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.TopicClose, Topic))
                bitMask += 8;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.TopicEdit, Topic))
                bitMask += 32;

            var _settings = ForumManager.Settings.LinkProvider;

            if (bitMask > 0)
            {
                sb.Append("<span class=\"menu-small\" onclick=\"javascript:ForumManager.ShowModeratorFunctions('" + Topic.ID + "'," + bitMask + "," + (int) Topic.Status + ",'" + _settings.EditTopic(Topic.ID) + "');\"></span>");
            }

            menuToggle.Text = sb.ToString();
        }
    }
}