/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            _forumManager = UserControls.Forum.Common.ForumManager.GetSettings(ForumManager.Settings.ID).ForumManager;

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
                Response.Redirect("default.aspx");


            var topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, idTopic);
            if (topic == null)
                Response.Redirect("default.aspx");

            Topic = topic;

            var postListControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/PostListControl.ascx") as PostListControl;
            postListControl.SettingsID = ForumManager.Settings.ID;
            postListControl.Topic = topic;
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

            var _settings = UserControls.Forum.Common.ForumManager.GetSettings(ForumManager.Settings.ID).LinkProvider;

            if (bitMask > 0)
            {
                sb.Append("<span class=\"menu-small\" onclick=\"javascript:ForumManager.ShowModeratorFunctions('" + Topic.ID + "'," + bitMask + "," + (int) Topic.Status + ",'" + _settings.EditTopic(Topic.ID) + "');\"></span>");
            }

            menuToggle.Text = sb.ToString();
        }
    }
}