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
using System.Web.UI.WebControls;
using ASC.Web.Core.Utility.Skins;
using AjaxPro;
using ASC.Forum;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public partial class Topics : MainPage
    {
        protected string ForumTitle { get; set; }
        protected string ForumParentTitle { get; set; }
        protected string ForumParentURL { get; set; }
        protected string SubscribeStatus { get; set; }
        protected bool EnableDelete { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.TopicList);
            Utility.RegisterTypeForAjax(typeof(ForumEditor), this.Page);
            int idThread;
            if (!int.TryParse(Request["f"], out idThread))
                Response.Redirect("Default.aspx");

            var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, idThread);

            if (thread == null)
                Response.Redirect("Default.aspx");

            if (thread.TopicCount > 0)
            {
                var topicsControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicListControl.ascx") as UserControls.Forum.TopicListControl;
                topicsControl.SettingsID = ForumManager.Settings.ID;
                topicsControl.ThreadID = thread.ID;
                topicsControl.PaggingHolder = PagingHolder;
                topicsHolder.Controls.Add(topicsControl);
            }
            else
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("forums_icon.png", ForumManager.Settings.ModuleID),
                        Header = Resources.ForumResource.EmptyScreenTopicCaption,
                        Describe = Resources.ForumResource.EmptyScreenTopicText,
                        ButtonHTML = ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.TopicCreate, thread) ? String.Format("<a class='link underline blue plus' href='newpost.aspx?f=" + thread.ID + "&m=0'>{0}</a>", Resources.ForumResource.EmptyScreenTopicLink) : String.Empty
                    };

                topicsHolder.Controls.Add(emptyScreenControl);
            }

            Utility.RegisterTypeForAjax(typeof(Subscriber));
            var subscriber = new Subscriber();

            var isThreadSubscribe = subscriber.IsThreadSubscribe(thread.ID);
            var subscribeThreadLink = subscriber.RenderThreadSubscription(!isThreadSubscribe, thread.ID);
            SubscribeLinkBlock.Text = subscribeThreadLink;

            //var master = Master as ForumMasterPage;
            //     master.ActionsPlaceHolder.Controls.Add(new HtmlMenuItem(subscriber.RenderThreadSubscription(!isThreadSubscribe, thread.ID)));

            ForumTitle = thread.Title;
            ForumParentTitle = Resources.ForumResource.ForumsBreadCrumbs;
            ForumParentURL = "Default.aspx";

            Title = HeaderStringHelper.GetPageTitle((Master as ForumMasterPage).CurrentPageCaption ?? Resources.ForumResource.AddonName);
            EnableDelete = ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null);
            var sb = new StringBuilder();
            sb.Append("<div id=\"forumsActionsMenuPanel\" class=\"studio-action-panel topics\">");
            sb.Append("<ul class=\"dropdown-content\">");
            if (EnableDelete)
            {
                sb.Append("<li><a class=\"dropdown-item\" href=\"javascript:ForumMakerProvider.DeleteThreadTopic('" + thread.ID + "','" + thread.CategoryID + "');\">" + Resources.ForumResource.DeleteButton + "</a></li>");
            }
            sb.Append("</ul>");
            sb.Append("</div>");
            topicsHolder.Controls.Add(new Literal { Text = sb.ToString() });

            SubscribeStatus = isThreadSubscribe ? "subscribed" : "unsubscribed";
        }
    }
}