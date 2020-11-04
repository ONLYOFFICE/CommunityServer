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
using ASC.Common.Security;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.Forum
{
    [AjaxNamespace("ForumEditor")]
    public partial class ForumEditor : System.Web.UI.UserControl
    {
        protected bool HasCategories { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            EditCategoryContainer.Options.IsPopup = true;
            NewCategoryContainer.Options.IsPopup = true;
            NewThreadContainer.Options.IsPopup = true;
            EditThreadContainer.Options.IsPopup = true;

            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                Response.Redirect(ForumManager.Instance.PreviousPage.Url);
                return;
            }

            Utility.RegisterTypeForAjax(typeof(ForumEditor), Page);

            Page.Title = HeaderStringHelper.GetPageTitle((Page.Master as ForumMasterPage).CurrentPageCaption ?? Resources.ForumResource.AddonName);
        }

        protected string RenderForumCategories()
        {
            List<ThreadCategory> categories;
            List<Thread> threads;
            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

            var sb = new StringBuilder();

            HasCategories = categories.Count > 0;

            if (HasCategories)
            {
                foreach (var category in categories)
                    sb.Append(RenderForumCategory(category, threads.FindAll(t => t.CategoryID == category.ID)));
            }
            else
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("forums_icon.png", ForumManager.Settings.ModuleID),
                        Header = Resources.ForumResource.EmptyScreenForumCaption,
                        Describe = Resources.ForumResource.EmptyScreenForumText,
                        ButtonHTML = String.Format("<a class='link underline blue plus' href='NewForum.aspx'>{0}</a>", Resources.ForumResource.EmptyScreenForumLink)
                    };
                EmptyContent.Controls.Add(emptyScreenControl);
            }

            return sb.ToString();
        }

        public static string RenderForumCategory(ThreadCategory category, List<Thread> threads)
        {
            var sb = new StringBuilder();

            sb.Append("<div class='borderBase cornerAll tintLight' id='forum_categoryBox_" + category.ID + "' name='" + category.ID + "' style='margin-bottom:5px;'>");
            sb.Append("<table cellpadding=0 cellspacing=0 style='width:100%;'>");
            sb.Append("<tr valign=top>");
            sb.Append("<td id='forum_categoryBoxHandle_" + category.ID + "' class='moveBackground' align=center style='padding-bottom:15px; width:10px;'>");

            sb.Append("<a href=\"javascript:ForumMakerProvider.ToggleThreadCategory('" + category.ID + "');\"><img id=\"forum_categoryState_" + category.ID + "\" style=\"border:0px;\" align='absmiddle' src=\"" + WebImageSupplier.GetAbsoluteWebPath("minus.png") + "\"/></a>");
            sb.Append("</td>");
            sb.Append("<td style='padding-left:10px;'>");

            sb.Append("<div class='clearFix' style='padding:3px;'>");

            sb.Append("<div style='float:left; width:70%; max-width:600px; overflow: hidden;'>");
            sb.Append("<div class='headerPanel'>" + category.Title.HtmlEncode() + "</div>");
            sb.Append("<div class='describe-text' style='margin-top:3px;'>" + category.Description.HtmlEncode().Replace("\r\n", "</br>").Replace("\n", "</br>").Replace("  ", " &nbsp;") + "</div>");


            sb.Append("</div>");


            sb.Append("<div align=right style='margin-right:8px; padding-top:5px; float:right;'>");

            sb.Append("<a class = 'link' href=\"javascript:ForumMakerProvider.ShowEditCategoryDialog('" + category.ID + "','" + category.Title.HtmlEncode().ReplaceSingleQuote() + "','" + category.Description.HtmlEncode().ReplaceSingleQuote() + "');\">" + Resources.ForumResource.EditShortButton + "</a>");

            sb.Append("<span class='splitter'>|</span>");

            sb.Append("<a class = 'link' href=\"javascript:ForumMakerProvider.RemoveCategory('" + category.ID + "');\">" + Resources.ForumResource.DeleteButton + "</a>");

            sb.Append("</div>");
            sb.Append("</div>");


            sb.Append("<div id='forum_threadListBox_" + category.ID + "' name='" + category.ID + "'>");

            threads.Sort((t1, t2) => Comparer<int>.Default.Compare(t1.SortOrder, t2.SortOrder));

            foreach (var thread in threads)
                sb.Append(RenderThread(thread));

            sb.Append("<div id='forum_threadBox_" + Guid.NewGuid() + "' style='display:none;' name='empty'></div>&nbsp;");
            sb.Append("</div>");


            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</table>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private static string RenderThread(Thread thread)
        {
            var sb = new StringBuilder();

            sb.Append("<div id='forum_threadBox_" + thread.ID + "' class='borderBase tintMedium cornerAll' name='" + thread.ID + "' style='margin:5px 5px 0px 5px;'>");

            sb.Append("<table style ='width:100%;' cellpadding=0 cellspacing=0>");
            sb.Append("<tr valign=top>");
            sb.Append("<td id='forum_threadBoxHandle_" + thread.ID + "' class='moveBackground' style='width:10px;'>&nbsp;</td>");
            sb.Append("<td style='padding-left:10px;'>");

            sb.Append("<div class='clearFix' style='padding:3px;'>");

            sb.Append("<div style='float:left; width:70%; max-width:600px; overflow: hidden;'><a class='link bold' href='topics.aspx?f=" + thread.ID + "'>" + thread.Title.HtmlEncode() + "</a>");
            sb.Append("<div class='describe-text' style='margin-top:3px;'>" + thread.Description.HtmlEncode().Replace("\r\n", "</br>").Replace("\n", "</br>").Replace("  ", " &nbsp;") + "</div>");
            sb.Append("</div>");

            sb.Append("<div align=right style='margin-right:3px; padding-top:5px; float:right;'>");

            //edit
            sb.Append("<a class = 'link' href=\"javascript:ForumMakerProvider.ShowEditThreadDialog('" + thread.ID + "','" + thread.CategoryID + "','"
                      + thread.Title.HtmlEncode().ReplaceSingleQuote() + "','" + thread.Description.HtmlEncode().ReplaceSingleQuote() + "');\">" + Resources.ForumResource.EditShortButton + "</a>");
            sb.Append("<span class='splitter'>|</span>");

            //remove
            sb.Append("<a class = 'link' href=\"javascript:ForumMakerProvider.DeleteThread('" + thread.ID + "','" + thread.CategoryID + "');\">" + Resources.ForumResource.DeleteButton + "</a>");


            sb.Append("</div>");

            sb.Append("</div>");


            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private string RenderModerators(ISecurityObjectId securityObj)
        {
            var isCategory = securityObj is ThreadCategory;
            var sb = new StringBuilder();

            sb.Append("<div style='margin:10px 0px 5px 0px;'><span style='font-weight:bolder;'>" + Resources.ForumResource.Moderators + ":&nbsp;</span>");
            sb.Append("<a href=\"javascript:ForumMakerProvider.SelectModerators('" + securityObj.SecurityId + "'," + (isCategory ? "true" : "false") + ");\">" + Resources.ForumResource.MakeModeratorsButton + "</a>");

            sb.Append("<div id=\"forum_modNames" + (isCategory ? "Category" : "Thread") + "_" + securityObj.SecurityId + "\" style='padding:5px;'>");

            var users = new List<UserInfo>();
            var groups = new List<GroupInfo>();
            //GetSubjects(securityObj, ASC.Common.Security.Authorizing.Constants.PowerUser, out users, out groups);

            var value = new StringBuilder();

            var isFirst = true;
            foreach (var user in users)
            {
                if (!isFirst)
                {
                    sb.Append(", ");
                    value.Append(",");
                }
                else
                    isFirst = false;

                sb.Append(user.DisplayUserName());
                value.Append(user.ID);
            }

            foreach (var group in groups)
            {
                if (!isFirst)
                    sb.Append(", ");
                else
                    isFirst = false;

                sb.Append(group.Name.HtmlEncode());
            }

            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<input id='forum_moderators" + (isCategory ? "Category" : "Thread") + "_" + securityObj.SecurityId + "' value='" + value.ToString() + "' type='hidden'/>");

            return sb.ToString();
        }

        private string RenderVisibleList(ISecurityObjectId securityObj)
        {
            var isCategory = securityObj is ThreadCategory;
            var sb = new StringBuilder();

            sb.Append("<div style='margin:10px 0px 5px 0px;'><span style='font-weight:bolder;'>" + Resources.ForumResource.VisibleList + ":&nbsp;</span>");
            sb.Append("<a href=\"javascript:ForumMakerProvider.SelectVisibleList('" + securityObj.SecurityId + "'," + (isCategory ? "true" : "false") + ");\">" + Resources.ForumResource.MakeModeratorsButton + "</a>");

            sb.Append("<div id=\"forum_vlNames" + (isCategory ? "Category" : "Thread") + "_" + securityObj.SecurityId + "\" style='padding:5px;'>");

            List<UserInfo> users = null;
            List<GroupInfo> groups = null;

            GetSubjects(out users, out groups);

            var value = new StringBuilder();

            var isFirst = true;
            var isEmpty = true;
            foreach (var user in users)
            {
                if (!isFirst)
                {
                    sb.Append(", ");
                    value.Append(",");
                }
                else
                    isFirst = false;

                sb.Append(user.DisplayUserName());
                value.Append(user.ID);
                isEmpty = false;
            }

            foreach (var group in groups)
            {
                if (!isFirst)
                    sb.Append(", ");
                else
                    isFirst = false;

                sb.Append(group.Name.HtmlEncode());
                isEmpty = false;
            }

            if (isEmpty)
                sb.Append(Resources.ForumResource.All);

            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<input id='forum_vl" + (isCategory ? "Category" : "Thread") + "_" + securityObj.SecurityId + "' value='" + value.ToString() + "' type='hidden'/>");

            return sb.ToString();
        }

        private static void GetSubjects(out List<UserInfo> users, out List<GroupInfo> groups)
        {
            users = new List<UserInfo>();
            groups = new List<GroupInfo>();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveModerators(int id, bool isCategory, string userIDs)
        {
            var resp = new AjaxResponse
                {
                    rs2 = id.ToString(),
                    rs4 = isCategory ? "1" : "0"
                };
            try
            {
                throw new NotSupportedException();
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = "<div class='errorBox'>" + e.Message.HtmlEncode() + "</div>";
            }

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveMembers(int id, bool isCategory, string userIDs)
        {
            var resp = new AjaxResponse
                {
                    rs2 = id.ToString(),
                    rs4 = isCategory ? "1" : "0"
                };

            try
            {
                if (!ForumManager.Instance.ValidateAccessSecurityAction(ASC.Forum.ForumAction.GetAccessForumEditor, null))
                    throw new Exception(Resources.ForumResource.ErrorAccessDenied);

                List<ThreadCategory> categories;
                List<Thread> threads;
                ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + e.Message.HtmlEncode() + "</div>";
            }

            return resp;
        }

        #region forum & cat sorting

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string UpdateCategorySortOrder(string sortOrders)
        {
            try
            {
                if (!ForumManager.Instance.ValidateAccessSecurityAction(ASC.Forum.ForumAction.GetAccessForumEditor, null))
                    throw new Exception(Resources.ForumResource.ErrorAccessDenied);

                List<ThreadCategory> categories;
                List<Thread> threads;
                ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

                foreach (var so in sortOrders.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var cid = Convert.ToInt32(so.Split(':')[0]);
                    var order = Convert.ToInt32(so.Split(':')[1]);
                    var category = categories.Find(c => c.ID == cid);

                    if (category != null)
                    {
                        category.SortOrder = order;
                        ForumDataProvider.UpdateThreadCategory(TenantProvider.CurrentTenantID, category.ID,
                                                               category.Title, category.Description, category.SortOrder);
                    }
                }

                return "ok";
            }
            catch (Exception e)
            {
                return e.Message.HtmlEncode();
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string UpdateThreadSortOrder(string sortOrders)
        {
            try
            {
                if (!ForumManager.Instance.ValidateAccessSecurityAction(ASC.Forum.ForumAction.GetAccessForumEditor, null))
                    throw new Exception(Resources.ForumResource.ErrorAccessDenied);

                List<ThreadCategory> categories;
                List<Thread> threads;
                ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, out categories, out threads);

                foreach (var categoryItem in sortOrders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var cid = Convert.ToInt32(categoryItem.Split('@')[0]);
                    var threadStr = categoryItem.Split('@')[1];

                    var category = categories.Find(c => c.ID == cid);
                    if (category != null)
                    {
                        foreach (var thrItem in threadStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            int tid = Convert.ToInt32(thrItem.Split(':')[0]);
                            int order = Convert.ToInt32(thrItem.Split(':')[1]);
                            var thread = threads.Find(t => t.ID == tid);
                            if (thread != null)
                            {
                                thread.CategoryID = category.ID;
                                thread.SortOrder = order;
                                ForumDataProvider.UpdateThread(TenantProvider.CurrentTenantID, thread.ID,
                                                               thread.CategoryID, thread.Title, thread.Description, thread.SortOrder);
                            }
                        }
                    }
                }

                return "ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        #region cat actions

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse CreateCategory(string name, string description)
        {
            var resp = new AjaxResponse();
            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class='errorBox'>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
                return resp;
            }

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(name.Trim()))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class='errorBox'>" + Resources.ForumResource.ErrorEmptyName + "</div>";
                return resp;
            }

            var category = new ThreadCategory()
                {
                    Title = name.Trim(),
                    Description = description ?? "",
                    SortOrder = 100
                };

            try
            {
                category.ID = ForumDataProvider.CreateThreadCategory(TenantProvider.CurrentTenantID, category.Title, category.Description, category.SortOrder);

                resp.rs1 = "1";
                resp.rs2 = RenderForumCategory(category, new List<Thread>());
            }
            catch
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.ForumResource.ErrorCreateThreadCategory + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveCategory(int id, string name, string description)
        {
            var resp = new AjaxResponse
                {
                    rs2 = id.ToString()
                };

            List<Thread> threads;
            var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, id, out threads);

            if (category == null || !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs3 = "<div'>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
                return resp;
            }

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(name.Trim()))
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorEmptyName + "</div>";
                return resp;
            }


            category.Title = name.Trim();
            category.Description = description ?? "";

            try
            {
                ForumDataProvider.UpdateThreadCategory(TenantProvider.CurrentTenantID, category.ID, category.Title, category.Description, category.SortOrder);
                resp.rs1 = "1";
                resp.rs3 = RenderForumCategory(category, threads);
            }
            catch
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorEditThreadCategory + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeleteThreadCategory(int id)
        {
            var resp = new AjaxResponse
                {
                    rs2 = id.ToString()
                };

            List<Thread> threads;
            var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, id, out threads);
            if (category == null)
            {
                resp.rs1 = "1";
                return resp;
            }
            if (!ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                RemoveDataHelper.RemoveThreadCategory(category);
                resp.rs1 = "1";
            }
            catch (Exception ex)
            {
                resp.rs1 = "0";
                resp.rs3 = ex.Message.HtmlEncode();
            }
            return resp;
        }

        #endregion

        #region thread actions

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoCreateThread(int categoryId, string name, string description)
        {
            var resp = new AjaxResponse
                {
                    rs2 = categoryId.ToString()
                };

            List<Thread> threads;
            var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, categoryId, out threads);

            if (category == null || !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
                return resp;
            }

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(name.Trim()))
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorEmptyName + "</div>";
                return resp;
            }

            var thread = new Thread()
                {
                    Title = name.Trim(),
                    Description = description ?? "",
                    SortOrder = 100,
                    CategoryID = category.ID
                };

            try
            {
                thread.ID = ForumDataProvider.CreateThread(TenantProvider.CurrentTenantID, thread.CategoryID, thread.Title,
                                                           thread.Description, thread.SortOrder);
                threads.Add(thread);
                resp.rs1 = "1";
                resp.rs3 = RenderForumCategory(category, threads);
            }
            catch
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorEditThreadCategory + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveThread(int id, int categoryID, string name, string description)
        {
            var resp = new AjaxResponse
                {
                    rs2 = categoryID.ToString()
                };

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(name.Trim()))
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorEmptyName + "</div>";
                return resp;
            }

            try
            {
                var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, id);

                if (thread == null || !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
                {
                    resp.rs1 = "0";
                    resp.rs3 = "<div>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
                    return resp;
                }

                thread.Title = name.Trim();
                thread.Description = description ?? "";
                thread.CategoryID = categoryID;

                ForumDataProvider.UpdateThread(TenantProvider.CurrentTenantID, thread.ID, thread.CategoryID,
                                               thread.Title, thread.Description, thread.SortOrder);

                List<Thread> threads;
                var category = ForumDataProvider.GetCategoryByID(TenantProvider.CurrentTenantID, thread.CategoryID, out threads);

                resp.rs1 = "1";
                resp.rs3 = RenderForumCategory(category, threads);
            }
            catch
            {
                resp.rs1 = "0";
                resp.rs3 = "<div>" + Resources.ForumResource.ErrorAccessDenied + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeleteThread(int threadID, int categoryID)
        {
            var resp = new AjaxResponse
                {
                    rs2 = threadID.ToString(),
                    rs3 = categoryID.ToString()
                };

            var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, threadID);

            if (thread == null ||
                !ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null))
            {
                resp.rs1 = "0";
                resp.rs4 = Resources.ForumResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                RemoveDataHelper.RemoveThread(thread);
                resp.rs1 = "1";
            }
            catch (Exception ex)
            {
                resp.rs1 = "0";
                resp.rs4 = ex.Message.HtmlEncode();
            }
            return resp;
        }

        #endregion
    }
}