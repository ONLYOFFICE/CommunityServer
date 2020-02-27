/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Forum;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.UserControls.Forum
{
    public partial class ThreadCategoryListControl : System.Web.UI.UserControl
    {
        public List<ThreadCategory> Categories { get; set; }
        public List<Thread> Threads { get; set; }
        public Guid SettingsID { get; set; }
        protected Settings _settings { get; set; }

        public ThreadCategoryListControl()
        {
            Categories = new List<ThreadCategory>();
            Threads = new List<Thread>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _settings = Community.Forum.ForumManager.Settings;

            _categoryRepeater.DataSource = Categories;
            _categoryRepeater.ItemDataBound += new RepeaterItemEventHandler(CategoryRepeater_ItemDataBound);
            _categoryRepeater.DataBind();
        }

        private void CategoryRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var threadRepeater = (Repeater)e.Item.FindControl("_threadRepeater");

                var category = (e.Item.DataItem as ThreadCategory);
                threadRepeater.DataSource = Threads.FindAll(t => t.CategoryID == category.ID);
                threadRepeater.DataBind();
            }
        }

        protected string RenderRecentUpdate(Thread thread)
        {
            if (thread.RecentPostID == 0)
                return "";

            var sb = new StringBuilder();
            sb.Append("<div><a class = 'link' title=\"" + HttpUtility.HtmlEncode(thread.RecentTopicTitle) + "\" href=\"" + _settings.LinkProvider.PostList(thread.RecentTopicID) + "\">" + HttpUtility.HtmlEncode(HtmlUtil.GetText(thread.RecentTopicTitle, 20)) + "</a></div>");
            //sb.Append("<div style='margin-top:5px;overflow: hidden;width: 180px;'>" + CoreContext.UserManager.GetUsers(thread.RecentPosterID).RenderProfileLink(_settings.ProductID, "describe-text", "link gray") + "</div>");
            sb.Append("<div style='margin-top:5px;overflow: hidden; max-width: 180px;'>" + ASC.Core.Users.StudioUserInfoExtension.RenderCustomProfileLink(CoreContext.UserManager.GetUsers(thread.RecentPosterID), "describe-text", "link gray") + "</div>");
            sb.Append("<div style='margin-top:5px;'>");
            sb.Append("<span class='text-medium-describe'>" + DateTimeExtension.AgoSentence(thread.RecentPostCreateDate) + "</span>");
            sb.Append("<a href=\"" + _settings.LinkProvider.RecentPost(thread.RecentPostID, thread.RecentTopicID, thread.RecentTopicPostCount) + "\"><img hspace=\"3\" align=\"absmiddle\" alt=\"&raquo;\" title=\"&raquo;\" border=\"0\" src=\"" + WebImageSupplier.GetAbsoluteWebPath("goto.png", _settings.ImageItemID) + "\"/></a>");
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}