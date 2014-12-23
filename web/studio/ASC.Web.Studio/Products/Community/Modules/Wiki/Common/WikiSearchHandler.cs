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
using System.Collections.Generic;
using System.Web;
using ASC.Common.Utils;
using ASC.Web.Community.Product;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.UserControls.Wiki.UC;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiSearchHandler : BaseSearchHandlerEx
    {
        public override SearchResultItem[] Search(string text)
        {
            var list = new List<SearchResultItem>();
            var defPageHref = VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath);

            foreach (var page in new WikiEngine().SearchPagesByContent(text))
            {
                var pageName = page.PageName;
                if (string.IsNullOrEmpty(pageName))
                {
                    pageName = WikiResource.MainWikiCaption;
                }

                list.Add(new SearchResultItem
                    {
                        Name = pageName,
                        Description = HtmlUtil.GetText(
                            EditPage.ConvertWikiToHtml(page.PageName, page.Body, defPageHref,
                                                       WikiSection.Section.ImageHangler.UrlFormat, TenantProvider.CurrentTenantID), 120),
                        URL = ActionHelper.GetViewPagePath(defPageHref, page.PageName),
                        Date = page.Date
                    });
            }
            return list.ToArray();
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "WikiLogo16.png", PartID = WikiManager.ModuleId }; }
        }

        public override string SearchName
        {
            get { return WikiManager.SearchDefaultString; }
        }

        public override Guid ModuleID
        {
            get { return WikiManager.ModuleId; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override IItemControl Control
        {
            get { return new ResultsView(); }
        }
    }
}