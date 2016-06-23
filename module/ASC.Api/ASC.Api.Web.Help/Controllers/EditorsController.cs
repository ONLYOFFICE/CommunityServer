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
using ASC.Api.Web.Help.DocumentGenerator;
using ASC.Api.Web.Help.Helpers;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ASC.Api.Web.Help.Controllers
{
    [Redirect]
    public class EditorsController : AsyncController
    {
        private readonly string[] _actionMap = new[]
            {
                "Advanced",
                "Alfresco",
                "Basic",
                "Callback",
                "Config",
                "Config/Document",
                "Config/Document/Info",
                "Config/Document/Permissions",
                "Config/Editor",
                "Config/Editor/Customization",
                "Config/Editor/Embedded",
                "Config/Events",
                "Confluence",
                "Conversion",
                "ConversionApi",
                "Example",
                "Example/Java",
                "Example/Nodejs",
                "Example/Php",
                "Example/Ruby",
                "Example/Csharp",
                "Hardware",
                "HowItWorks",
                "Methods",
                "Open",
                "Plugins",
                "Save",
            };

        public ActionResult Search(string query)
        {
            var result = new List<SearchResult>();

            foreach (var action in _actionMap)
            {
                var actionString = action.ToLower();
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


        public ActionResult Navigation()
        {
            return View();
        }


        public ActionResult Index()
        {
            return View("Basic");
        }


        public ActionResult Alfresco()
        {
            return View();
        }

        public ActionResult Advanced()
        {
            return View();
        }

        public ActionResult Basic()
        {
            return View();
        }

        public ActionResult Callback()
        {
            return View();
        }

        public ActionResult Config(string catchall)
        {
            if (!_actionMap.Contains("config/" + catchall, StringComparer.OrdinalIgnoreCase))
            {
                catchall = null;
            }
            return View("Config", (object) catchall);
        }

        public ActionResult Confluence()
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

        public ActionResult Example(string catchall)
        {
            if (!_actionMap.Contains("example/" + catchall, StringComparer.OrdinalIgnoreCase))
            {
                catchall = null;
            }
            return View("Example", (object)catchall);
        }

        public ActionResult DemoPreview()
        {
            var directoryInfo = new DirectoryInfo(Request.MapPath("~/app_data"));

            var examples = directoryInfo.GetFiles("*.zip", SearchOption.TopDirectoryOnly).Select(fileInfo => fileInfo.Name).ToList();

            return View(examples);
        }

        public ActionResult Hardware()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Methods()
        {
            return View();
        }

        public ActionResult Open()
        {
            return View();
        }

        public ActionResult Plugins()
        {
            return View();
        }

        public ActionResult Save()
        {
            return View();
        }
    }
}