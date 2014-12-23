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
    public class PartnersController : AsyncController
    {
        private enum SectionType
        {
            Partners,
            Clients,
            Portals,
            Keys,
            Invoices
        }

        private enum ActionType
        {
            Basic,
            Authentication,
            ActivateKey,
            ChangeInvoiceStatus,
            ChangePortalStatus,
            CreateButton,
            CreateClient,
            GenerateKeys,
            GenerateKeysMin,
            GetCurrentPartner,
            GetInvoiceKeys,
            GetPartner,
            GetPartnerInvoices,
            GetPartnerTariffs,
            RegisterPortal,
            RequestClientPayment
        }

        public ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
                return View("basic");

            SectionType section;
            if (Enum.TryParse(id, true, out section))
                return View("index", (object)id);

            return View("sectionnotfound");
        }

        public ActionResult Navigation()
        {
            return View();
        }

        public ActionResult Basic()
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
                    result.Add(new SearchResult{
                        Module = "partners",
                        Name = actionString,
                        Resource = Highliter.HighliteString(header, query).ToHtmlString(),
                        Description = Highliter.HighliteString(descr, query).ToHtmlString(),
                        Url = Url.Action(actionString, "partners")
                    });
                }
            }

            ViewData["query"] = query ?? string.Empty;
            ViewData["result"] = result;
            return View(new Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>>());
        }

        public ActionResult ActivateKey()
        {
            return View();
        }

        public ActionResult Authentication()
        {
            return View();
        }

        public ActionResult ChangeInvoiceStatus()
        {
            return View();
        }

        public ActionResult ChangePortalStatus()
        {
            return View();
        }

        public ActionResult CreateButton()
        {
            return View();
        }

        public ActionResult CreateClient()
        {
            return View();
        }

        public ActionResult GenerateKeys()
        {
            return View();
        }

        public ActionResult GenerateKeysMin()
        {
            return View();
        }

        public ActionResult GetCurrentPartner()
        {
            return View();
        }

        public ActionResult GetInvoiceKeys()
        {
            return View();
        }

        public ActionResult GetPartner()
        {
            return View();
        }

        public ActionResult GetPartnerInvoices()
        {
            return View();
        }

        public ActionResult GetPartnerTariffs()
        {
            return View();
        }

        public ActionResult RegisterPortal()
        {
            return View();
        }

        public ActionResult RequestClientPayment()
        {
            return View();
        }
    }
}
