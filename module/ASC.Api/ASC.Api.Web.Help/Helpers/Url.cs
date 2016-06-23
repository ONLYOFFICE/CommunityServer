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
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using ASC.Api.Web.Help.DocumentGenerator;

namespace ASC.Api.Web.Help.Helpers
{
    public static class Url
    {
        public static string DocUrl(this UrlHelper url, MsDocEntryPoint section, MsDocEntryPointMethod method, object controller)
        {
            var sectionName = section != null ? section.Name : null;
            return method == null ?
                       url.DocUrl(sectionName, null, null, null, controller) :
                       url.DocUrl(sectionName, method.Category, method.HttpMethod, method.Path, controller);
        }

        public static string DocUrl(this UrlHelper url, string section, string category, string type, string path, object controller)
        {
            if(controller == null) throw new ArgumentException();
            
            var routeName = GetRouteName(section, category, type, path);
            var routeValues = GetRouteValues(section, category, type, path, controller);
            
            return url.RouteUrl(routeName, routeValues);
        }



        public static object GetRouteValues(MsDocEntryPoint section, MsDocEntryPointMethod method, object controller)
        {
            var sectionName = section != null ? section.Name : null;
            return method == null ?
                       GetRouteValues(sectionName, null, null, null, controller) :
                       GetRouteValues(sectionName, method.Category, method.HttpMethod, method.Path, controller);
        }

        public static object GetRouteValues(string section, string category, string type, string path, object controller)
        {
            if (controller == null) throw new ArgumentException();
            
            if (string.IsNullOrEmpty(section))
                return new
                    {
                        controller = controller ?? UrlParameter.Optional,
                        id = UrlParameter.Optional
                    };
            
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(path))
                return new
                    {
                        controller = controller ?? UrlParameter.Optional,
                        section = string.IsNullOrEmpty(section) ? UrlParameter.Optional : (object) section.ToLowerInvariant(),
                        category = string.IsNullOrEmpty(category) ? UrlParameter.Optional : (object) category.ToLowerInvariant()
                    };

            return new
                {
                    controller = controller ?? UrlParameter.Optional,
                    section = string.IsNullOrEmpty(section) ? UrlParameter.Optional : (object) section.ToLowerInvariant(),
                    type = string.IsNullOrEmpty(type) ? UrlParameter.Optional : (object) type.ToLowerInvariant(),
                    url = string.IsNullOrEmpty(path) ? UrlParameter.Optional : (object) path.ToLowerInvariant()
                };
        }



        public static string GetRouteName(MsDocEntryPoint section, MsDocEntryPointMethod method)
        {
            var sectionName = section != null ? section.Name : null;
            return method == null ?
                       GetRouteName(sectionName, null, null, null) :
                       GetRouteName(sectionName, method.Category, method.HttpMethod, method.Path);
        }

        public static string GetRouteName(string section, string category, string type, string path)
        {
            if (string.IsNullOrEmpty(section))
                return "Default";

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(path))
                return "Sections";

            return "Methods";
        }



        public static string GetActionName(MsDocEntryPoint section, MsDocEntryPointMethod method)
        {
            var sectionName = section != null ? section.Name : null;
            return method == null ?
                       GetActionName(sectionName, null, null, null) :
                       GetActionName(sectionName, method.Category, method.HttpMethod, method.Path);
        }

        public static string GetActionName(string section, string category, string type, string path)
        {
            if (string.IsNullOrEmpty(section))
                return "index";

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(path))
                return "section";

            return "method";
        }



        public static string GetDocUrl(MsDocEntryPoint section, MsDocEntryPointMethod method, object controller, RequestContext context)
        {
            var sectionName = section != null ? section.Name : null;
            return method == null ?
                       GetDocUrl(sectionName, null, null, null, controller, context) :
                       GetDocUrl(sectionName, method.Category, method.HttpMethod, method.Path, controller, context);
        }
        
        public static string GetDocUrl(string section, string category, string type, string path, object controller, RequestContext context)
        {
            if (controller == null || context == null) throw new ArgumentException();
            
            var routeName = GetRouteName(section, category, type, path);
            var actionName = GetActionName(section, category, type, path);
            var routeValues = new RouteValueDictionary(GetRouteValues(section, category, type, path, controller));

            return UrlHelper.GenerateUrl(routeName, actionName, (string)controller, routeValues, RouteTable.Routes, context, false);
        }
    }
}