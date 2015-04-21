/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
