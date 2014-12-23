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

using ASC.Api.Web.Help.Helpers;
using ASC.Api.Web.Help.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ASC.Api.Web.Help.Controllers
{
    [Redirect]
    public class PortalsController : AsyncController
    {
        private readonly BreadCrumbsBuilder _breadCrumbsBuilder;

        public PortalsController()
        {
            _breadCrumbsBuilder = new BreadCrumbsBuilder(this);
        }

        public ActionResult Index()
        {
            return View("basic");
        }

        public ActionResult Navigation()
        {
            return View(Documentation.GetAll());
        }

        public ActionResult Auth()
        {
            return View();
        }

        public ActionResult Basic()
        {
            return View();
        }

        public ActionResult Search(string query)
        {
            ViewData["query"] = query ?? string.Empty;
            return View(Documentation.Search(query ?? string.Empty));
        }

        public ActionResult Section(string section, string category)
        {
            if (string.IsNullOrEmpty(section))
                return View("sectionnotfound");

            var docsSection = Documentation.GetDocs(section);
            if (docsSection == null || !docsSection.Methods.Any())
                return View("sectionnotfound");

            const string controller = "portals";
            _breadCrumbsBuilder.Add(docsSection.Name, docsSection, null, controller);

            if (string.IsNullOrEmpty(category))
            {
                var sectionMethods = docsSection.Methods.Where(x => string.IsNullOrEmpty(x.Category)).ToList();
                if (sectionMethods.Any())
                {
                    return View("section", new SectionViewModel(docsSection, null, sectionMethods));
                }

                category = docsSection.Methods.OrderBy(x => x.Category).First(x => !string.IsNullOrEmpty(x.Category)).Category;
                return Redirect(Url.DocUrl(section, category, null, null, "portals"));
            }

            var categoryMethods = docsSection.Methods.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            if (categoryMethods.Any())
            {
                _breadCrumbsBuilder.Add(category, docsSection.Name, category, null, null, controller);
                return View("section", new SectionViewModel(docsSection, category, categoryMethods));
            }

            return View("sectionnotfound");
        }

        public ActionResult Method(string section, string type, string url)
        {
            if (string.IsNullOrEmpty(section))
                return View("sectionnotfound");

            var docsSection = Documentation.GetDocs(section);
            if (docsSection == null)
                return View("sectionnotfound");

            const string controller = "portals";
            _breadCrumbsBuilder.Add(docsSection.Name, docsSection, null, controller);

            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(url))
            {
                var method = docsSection.Methods.SingleOrDefault(x => x.Path.Equals(url, StringComparison.OrdinalIgnoreCase) && x.HttpMethod.Equals(type, StringComparison.OrdinalIgnoreCase));
                if (method != null)
                {
                    if (!string.IsNullOrEmpty(method.Category))
                        _breadCrumbsBuilder.Add(method.Category, docsSection.Name, method.Category, null, null, controller);

                    var text = string.IsNullOrEmpty(method.ShortName) ?
                        (string.IsNullOrEmpty(method.Summary) ? method.FunctionName : method.Summary) :
                        method.ShortName;

                    _breadCrumbsBuilder.Add(text, docsSection, method, controller);

                    return View("method", new MethodViewModel(docsSection, method));
                }
            }

            return View("methodnotfound");
        }
    }
}
