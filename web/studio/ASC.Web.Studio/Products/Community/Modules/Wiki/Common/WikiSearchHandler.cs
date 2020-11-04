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
            get { return new ImageOptions { ImageFileName = "wikilogo16.png", PartID = WikiManager.ModuleId }; }
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