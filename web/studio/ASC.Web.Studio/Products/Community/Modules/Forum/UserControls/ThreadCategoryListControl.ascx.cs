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