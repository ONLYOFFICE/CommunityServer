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
using System.Web.Mvc;
using ASC.Api.Web.Help.DocumentGenerator;
using ASC.Api.Web.Help.Helpers;
using HtmlAgilityPack;

namespace ASC.Api.Web.Help.Controllers
{
    [Redirect]
    public class EditorsController : AsyncController
    {
        private enum ActionType
        {
            Basic,
            HowItWorks,
            Config,
            Document,
            DocInfo,
            DocPermissions,
            Editor,
            Events,
            Open,
            Save,
            Conversion,
            ConversionApi,
            Hardware
        }
        
        public ActionResult Index()
        {
            return View("Basic");
        }

        public ActionResult Navigation()
        {
            return View();
        }

        public ActionResult Search(string query)
        {
            var result = new List<SearchResult>();

            foreach (var action in (ActionType[])Enum.GetValues(typeof(ActionType)))
            {
                var actionString = action.ToString().ToLower();
                var doc = new HtmlDocument();
                var html = this.RenderView(actionString, new ViewDataDictionary());
                doc.LoadHtml(html);
                var headerNode = doc.DocumentNode.SelectSingleNode("//span[@class='hdr']");
                var descrNode = doc.DocumentNode.SelectSingleNode("//p[@class='dscr']");
                var header = headerNode != null ? headerNode.InnerText : string.Empty;
                var descr = descrNode != null ? descrNode.InnerText : string.Empty;

                if (!string.IsNullOrEmpty(query) && doc.DocumentNode.InnerText.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                {
                    result.Add(new SearchResult
                    {
                        Module = "editors",
                        Name = Highliter.HighliteString(header, query).ToHtmlString(),
                        Resource = string.Empty,
                        Description = Highliter.HighliteString(descr, query).ToHtmlString(),
                        Url = Url.Action(actionString, "editors")
                    });
                }
            }

            ViewData["query"] = query ?? string.Empty;
            ViewData["result"] = result;
            return View(new Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>>());
        }

        public ActionResult Basic()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Config()
        {
            return View();
        }

        public ActionResult Document()
        {
            return View();
        }

        public ActionResult DocInfo()
        {
            return View();
        }

        public ActionResult DocPermissions()
        {
            return View();
        }

        public ActionResult Editor()
        {
            return View();
        }

        public ActionResult Events()
        {
            return View();
        }

        public ActionResult Open()
        {
            return View();
        }

        public ActionResult Save()
        {
            return View();
        }

        public ActionResult Conversion()
        {
            return View();
        }

        public ActionResult ConversionApi()
        {
            return View();
        }
        
        public ActionResult Hardware()
        {
            return View();
        }
    }
}