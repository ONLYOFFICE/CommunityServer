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
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ViewPage : BaseUserControl
    {
        private string _pageName = string.Empty;
        
        public string PageName
        {
            get { return PageNameUtil.Encode(_pageName); }
            set { _pageName = PageNameUtil.Decode(value); }
        }

        public int Version { get; set; }     

        public bool CanEditPage
        {
            get
            {
                if (ViewState["CanEditPage"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["CanEditPage"]);
            }
            set { ViewState["CanEditPage"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e) { }

        protected string RenderPageContent()
        {
            Page page;

            if (_pageName == null)
            {
                _pageName = string.Empty;
            }

            if (Version > 0)
            {
                page = Wiki.GetPage(_pageName, Version);
            }
            else
            {
                page = Wiki.GetPage(_pageName);
            }
            
            if (page == null)
            {
                return RenderEmptyPage();
            }

            RiseWikiPageLoaded(page);
            RisePublishVersionInfo(page);

            return HtmlWikiUtil.WikiToHtml(page.PageName, page.Body, Page.ResolveUrl(Request.AppRelativeCurrentExecutionFilePath), 
                Wiki.GetPagesAndFiles(page.Body), Page.ResolveUrl(ImageHandlerUrlFormat), 
                TenantId, CanEditPage && Version == 0 ? ConvertType.Editable : ConvertType.NotEditable);
        }

        protected string RenderEmptyPage()
        {
            RisePageEmptyEvent();
            return string.Empty; //HtmlWikiUtil.WikiToHtml(WikiUCResource.MainPage_EmptyPage);
        }
    }
}