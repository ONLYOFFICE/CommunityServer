/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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