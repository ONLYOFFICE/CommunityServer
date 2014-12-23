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
                Response.Redirect("default.aspx");

            var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, idThread);

            if (thread == null)
                Response.Redirect("default.aspx");

            if (thread.TopicCount > 0)
            {
                var topicsControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicListControl.ascx") as UserControls.Forum.TopicListControl;
                topicsControl.SettingsID = ForumManager.Settings.ID;
                topicsControl.ThreadID = thread.ID;
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
            ForumParentURL = "default.aspx";

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