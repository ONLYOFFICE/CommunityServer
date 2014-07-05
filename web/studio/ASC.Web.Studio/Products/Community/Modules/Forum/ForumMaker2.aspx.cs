/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ASC.Web.Studio;
using AjaxPro;
using ASC.Forum;
using System.Collections.Generic;
using ASC.Web.Studio.Utility;
using System.Text;

namespace ASC.Web.Community.Forum
{
    public partial class ForumMaker2 : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(ForumMaker));
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetCategoryList()
        {
            
            var categories = new List<ThreadCategory>();
            var threads = new List<Thread>();
            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='headerPanelSmall-splitter'><b>" + Resources.ForumResource.ThreadCategory + ":</b></div>");
            sb.Append("<select id='forum_fmCategoryID' onchange=\"ForumMakerProvider.SelectCategory();\" style='width:100%;' class='comboBox'>");
            sb.Append("<option value='-1'>" + Resources.ForumResource.TypeCategoryName + "</option>");
            foreach (var categ in categories)
            {
                sb.Append("<option value='" + categ.ID + "'>" + categ.Title.HtmlEncode() + "</option>");
            }
            sb.Append("</select>");
            //jq('#forum_fmCategoryList').html(result.value);

            //jq('#forum_fmMessage').html('');
            //jq('#forum_fmMessage').hide();

            //jq('#forum_fmCategoryName').val('');
            //jq('#forum_fmForumName').val('');
            //jq('#forum_fmForumDescription').val('');
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveThreadCategory(int categoryID, string categoryName, string threadName, string threadDescription, bool isRenderCategory)
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

                    thread.CategoryID = ForumDataProvider.CreateThreadCategory(TenantProvider.CurrentTenantID, categoryName.Trim(), "", 100);
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

                ForumActivityPublisher.NewThread(thread);
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
