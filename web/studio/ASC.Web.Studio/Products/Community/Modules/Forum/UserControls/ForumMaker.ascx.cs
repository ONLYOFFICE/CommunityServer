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