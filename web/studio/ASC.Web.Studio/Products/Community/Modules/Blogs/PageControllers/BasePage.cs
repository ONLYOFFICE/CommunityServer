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
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Blogs
{
    public abstract class BasePage : MainPage
    {
        protected int GetBlogMaxImageWidth
        {
            get { return 570; }
        }

        public static BlogsEngine GetEngine()
        {
            return BlogsEngine.GetEngine(TenantProvider.CurrentTenantID);
        }

        /// <summary>
        /// Page_Load of the Page Controller pattern.
        /// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnpatterns/html/ImpPageController.asp
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof (AddBlog));
            PageLoad();
            RenderScripts();
        }

        protected abstract void PageLoad();

        protected string GetLimitedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text.Length > ASC.Blogs.Core.Constants.MAX_TEXT_LENGTH ? text.Substring(0, ASC.Blogs.Core.Constants.MAX_TEXT_LENGTH) : text;
        }

        protected virtual string RenderRedirectUpload()
        {
            return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=blogs");
        }

        protected void RenderScripts()
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/js/blogs.js"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/asc/plugins/tagsautocompletebox.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/app_themes/default/blogstyle.css"));

            var script = @"
function createSearchHelper() {

	var ForumTagSearchHelper = new SearchHelper(
		'_txtTags', 
		'tagAutocompleteItem', 
		'tagAutocompleteSelectedItem', 
		'', 
		'', 
		'BlogsPage',
		'GetSuggest',
		'', 
		true,
		false
	);
}
";

            Page.ClientScript.RegisterClientScriptBlock(typeof (string), "blogsTagsAutocompleteInitScript", script, true);
        }
    }
}