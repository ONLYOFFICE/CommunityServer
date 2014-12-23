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
            _settings = ForumManager.GetSettings(SettingsID);

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