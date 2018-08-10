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

            var _settings = ForumManager.Settings.LinkProvider;

            if (bitMask > 0)
            {
                sb.Append("<span class=\"menu-small\" onclick=\"javascript:ForumManager.ShowModeratorFunctions('" + Topic.ID + "'," + bitMask + "," + (int) Topic.Status + ",'" + _settings.EditTopic(Topic.ID) + "');\"></span>");
            }

            menuToggle.Text = sb.ToString();
        }
    }
}