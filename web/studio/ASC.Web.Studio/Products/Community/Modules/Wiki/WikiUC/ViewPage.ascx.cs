/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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