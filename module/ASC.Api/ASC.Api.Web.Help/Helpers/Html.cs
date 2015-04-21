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
            var type = method.HttpMethod;
            var path = method.Path;
            var controller = helper.ViewContext.RequestContext.RouteData.Values["controller"];
            var context = helper.ViewContext.RequestContext;
            
            var spanMethod = new TagBuilder("span");
            spanMethod.AddCssClass("http-" + type.ToLowerInvariant());
            spanMethod.InnerHtml = HttpUtility.HtmlEncode(type);

            var tagBuilder = new TagBuilder("a")
            {
                InnerHtml = spanMethod.ToString(TagRenderMode.Normal) + "&nbsp;" + HttpUtility.HtmlEncode(path)
            };
            tagBuilder.AddCssClass("underline");
            tagBuilder.MergeAttribute("href", Url.GetDocUrl(section, method, controller, context));
            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString DocSectionLink(this HtmlHelper helper, MsDocEntryPoint section)
        {
            var controller = helper.ViewContext.RequestContext.RouteData.Values["controller"];
            var routeValues = Url.GetRouteValues(section, null, controller);
            return helper.RouteLink(section.Name, "Sections", routeValues, new { @class = "api-section" });
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
            var currentController = helper.ViewContext.RequestContext.RouteData.Values["controller"];
            return string.Equals((string)currentController, controller, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IfAction(this HtmlHelper helper, string action)
        {
            var currentAction = helper.ViewContext.RequestContext.RouteData.Values["controller"];
            return string.Equals((string)currentAction, action, StringComparison.OrdinalIgnoreCase);
        }

        public static object GetCurrentController(this HtmlHelper helper)
        {
            return helper.ViewContext.RequestContext.RouteData.Values["controller"];
        }

        public static object GetCurrentAction(this HtmlHelper helper)
        {
            return helper.ViewContext.RequestContext.RouteData.Values["action"];
        }
    }
}