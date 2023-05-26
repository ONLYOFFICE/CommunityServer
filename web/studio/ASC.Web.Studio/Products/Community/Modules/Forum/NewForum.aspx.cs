/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using AjaxPro;

using ASC.Forum;
using ASC.Web.Community.Modules.Forum.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Forum
{
    public partial class NewForum : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                Response.Redirect(ForumManager.Instance.PreviousPage);
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
            sb.Append("<div class='headerPanelSmall-splitter'><b>" + ForumResource.ThreadCategory + ":</b></div>");
            sb.Append("<select id='forum_fmCategoryID' onchange=\"ForumMakerProvider.SelectCategory();\" style='width:400px;' class='comboBox'>");
            sb.Append("<option value='-1'>" + ForumResource.TypeCategoryName + "</option>");
            foreach (var categ in categories)
            {
                sb.Append("<option value='" + categ.ID + "'>" + categ.Title.HtmlEncode() + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }
    }
}
