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
using System.Web;
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Web.Studio;
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

            return text.Length > Constants.MAX_TEXT_LENGTH ? text.Substring(0, Constants.MAX_TEXT_LENGTH) : text;
        }

        protected virtual string RenderRedirectUpload()
        {
            return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=blogs");
        }

        protected void RenderScripts()
        {
            Page.RegisterBodyScripts("~/Products/Community/Modules/Blogs/js/blogs.js",
                "~/Products/Community/js/tagsautocompletebox.js")
                .RegisterStyle("~/Products/Community/Modules/Blogs/App_Themes/default/blogstyle.css")
                .RegisterInlineScript(@"
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
");
        }
    }
}