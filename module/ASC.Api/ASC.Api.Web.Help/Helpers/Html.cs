/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ASC.Api.Web.Help.DocumentGenerator;

namespace ASC.Api.Web.Help.Helpers
{
    public static class Html
    {
        public static MvcHtmlString DocMethodLink(this HtmlHelper helper, MsDocEntryPoint section, MsDocEntryPointMethod method)
        {
            return DocMethodLink(helper, section, method, null);
        }

        public static MvcHtmlString DocMethodLink(this HtmlHelper helper, MsDocEntryPoint section, MsDocEntryPointMethod method, object htmlAttributes)
        {
            return DocMethodLink(helper, section.Name, method.HttpMethod, method.Path, htmlAttributes);
        }

        public static MvcHtmlString DocMethodLink(this HtmlHelper helper, string section, string method, string apiUrl)
        {
            return DocMethodLink(helper, section, method, apiUrl, null);
        }

        public static MvcHtmlString DocMethodLink(this HtmlHelper helper, string section, string method, string apiUrl, object htmlAttributes)
        {
            var spanMethod = new TagBuilder("span");
            spanMethod.AddCssClass("http-"+method.ToLowerInvariant());
            spanMethod.InnerHtml = !string.IsNullOrEmpty(method) ? HttpUtility.HtmlEncode(method) : string.Empty;

            var tagBuilder = new TagBuilder("a")
            {
                InnerHtml = spanMethod.ToString(TagRenderMode.Normal) +"&nbsp;"+ (!string.IsNullOrEmpty(apiUrl) ? HttpUtility.HtmlEncode(apiUrl) : string.Empty)
            };
            tagBuilder.AddCssClass("api-method");
            tagBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tagBuilder.MergeAttribute("href", Url.GetDocUrl(section, method, apiUrl, helper.ViewContext.RequestContext));
            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString DocSectionLink(this HtmlHelper helper, string section)
        {
            return helper.RouteLink(section, "Docs", Url.GetRouteValues(section, null, null),
                                    new {@class = "api-section"});
        }

        public static MvcHtmlString DocSectionLink(this HtmlHelper helper, MsDocEntryPoint section)
        {
            return helper.DocSectionLink(section.Name);
        }

        public static MvcHtmlString MenuActionLink(this HtmlHelper helper, string linkText, string action, string controller, string selectedClass)
        {
            return MenuActionLink(helper, linkText, action, controller, selectedClass, null);
        }

        public static MvcHtmlString MenuActionLink(this HtmlHelper helper, string linkText, string action, string controller, string selectedClass, object routeValues)
        {
            var url = UrlHelper.GenerateUrl("Default", action, controller, new RouteValueDictionary(routeValues),
                                  RouteTable.Routes, helper.ViewContext.RequestContext, false);
            object htmlAttrs = null;
            if (url.Equals(helper.ViewContext.RequestContext.HttpContext.Request.Url.AbsolutePath,StringComparison.OrdinalIgnoreCase))
            {
                htmlAttrs = new {@class = selectedClass};
            }
            return helper.ActionLink(linkText, action, controller, routeValues, htmlAttrs);
        }

        public static bool IfController(this HtmlHelper helper, string controller)
        {
            if (ReferenceEquals(helper.ViewContext.RequestContext.RouteData.Values["controller"], controller))
            {
                return true;
            }
            return false;
        }

        public static bool IfAction(this HtmlHelper helper, string action)
        {
            if (ReferenceEquals(helper.ViewContext.RequestContext.RouteData.Values["action"], action))
            {
                return true;
            }
            return false;
        }
    }
}