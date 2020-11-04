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
using ASC.Web.Studio;
using AjaxPro;
using ASC.Forum;
using System.Collections.Generic;
using ASC.Web.Studio.Utility;
using System.Text;

namespace ASC.Web.Community.Forum
{
    public partial class NewForum : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                Response.Redirect(ForumManager.Instance.PreviousPage.Url);
                return;
            }

            Utility.RegisterTypeForAjax(typeof(ForumMaker));
        }

        public string GetCategoryList()
        {
            var categories = new List<ThreadCategory>();
            var threads = new List<Thread>();
            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='headerPanelSmall-splitter'><b>" + Resources.ForumResource.ThreadCategory + ":</b></div>");
            sb.Append("<select id='forum_fmCategoryID' onchange=\"ForumMakerProvider.SelectCategory();\" style='width:400px;' class='comboBox'>");
            sb.Append("<option value='-1'>" + Resources.ForumResource.TypeCategoryName + "</option>");
            foreach (var categ in categories)
            {
                sb.Append("<option value='" + categ.ID + "'>" + categ.Title.HtmlEncode() + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }
    }
}
