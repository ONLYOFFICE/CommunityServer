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
using AjaxPro;
using ASC.Core;
using ASC.Forum;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Forum
{  
    [AjaxNamespace("ForumMaker")]
    public partial class ForumMaker : System.Web.UI.UserControl
    {
        public static string Location
        {
            get
            {
                return ForumManager.BaseVirtualPath + "/UserControls/ForumMaker.ascx";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(this.GetType());           
            _forumMakerContainer.Options.IsPopup = true;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetCategoryList()
        {   
            var categories = new List<ThreadCategory>();
            var threads = new List<Thread>();
            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

            StringBuilder sb = new StringBuilder();
            sb.Append("<div>"+Resources.ForumResource.ThreadCategory+":</div>");
            sb.Append("<select id='forum_fmCategoryID' onchange=\"ForumMakerProvider.SelectCategory();\" style='margin-top:3px; width:99%;' class='comboBox'>");
            sb.Append("<option value='-1'>"+Resources.ForumResource.TypeCategoryName+"</option>");
            foreach (var categ in categories)
            {
                sb.Append("<option value='"+categ.ID+"'>"+categ.Title.HtmlEncode()+"</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveThreadCategory(int categoryID, string categoryName, string categoryDescription, string threadName, string threadDescription, bool isRenderCategory)
        {
            AjaxResponse resp = new AjaxResponse();

            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
                return resp;
            }

            if (String.IsNullOrEmpty(threadName) || String.IsNullOrEmpty(threadName.Trim()))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.ForumResource.ErrorThreadEmptyName + "</div>";
                return resp;
            }

            try
            {
                var thread = new Thread()
                {
                    Title = threadName.Trim(),
                    Description = threadDescription ?? "",
                    SortOrder = 100,
                    CategoryID = categoryID
                };

                if (thread.CategoryID == -1)
                {
                    if (String.IsNullOrEmpty(categoryName) || String.IsNullOrEmpty(categoryName.Trim()))
                    {
                        resp.rs1 = "0";
                        resp.rs2 = "<div>" + Resources.ForumResource.ErrorCategoryEmptyName + "</div>";
                        return resp;
                    }

                    thread.CategoryID = ForumDataProvider.CreateThreadCategory(TenantProvider.CurrentTenantID, categoryName.Trim(), categoryDescription.Trim(), 100);
                }

                thread.ID = ForumDataProvider.CreateThread(TenantProvider.CurrentTenantID, thread.CategoryID,
                                                           thread.Title, thread.Description, thread.SortOrder);

                resp.rs1 = "1";
                resp.rs2 = thread.CategoryID.ToString();

                if (isRenderCategory)
                {
                    var threads = new List<Thread>();
                    var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, thread.CategoryID, out threads);

                    resp.rs3 = ForumEditor.RenderForumCategory(category, threads);
                }

                resp.rs4 = (categoryID == -1) ? "0" : "1";
                resp.rs5 = thread.ID.ToString();
            }
            catch (Exception ex)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + ex.Message.HtmlEncode() + "</div>";
            }

            return resp;
        }
    }    
}