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


using ASC.Api.Web.Help.Helpers;
using log4net;
using log4net.Config;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ASC.Api.Web.Help
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static readonly CacheManifest CacheManifest = new CacheManifest();
        private static readonly object locker = new object();
        private static volatile bool initialized;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Cache", "web.appcache", new { controller = "CacheManifest", action = "GetCacheManifest" });

            routes.MapRoute(
                "Methods", // Route name
                "{controller}/method/{section}/{type}/{*url}", // URL with parameters
                new
                    {
                        controller = "portals",
                        action = "method",
                        section = UrlParameter.Optional,
                        type = UrlParameter.Optional,
                        url = UrlParameter.Optional
                    } // Parameter defaults
                );

            routes.MapRoute(
                "Sections", // Route name
                "{controller}/section/{section}/{category}", // URL with parameters
                new
                    {
                        controller = "portals",
                        action = "section",
                        section = UrlParameter.Optional,
                        category = UrlParameter.Optional
                    } // Parameter defaults
                );

            routes.MapRoute(
                "Partners", // Route name
                "partners/{action}/{id}", // URL with parameters
                new
                    {
                        controller = "partners",
                        action = "index",
                        id = UrlParameter.Optional
                    } // Parameter defaults
                );

            routes.MapRoute(
                "ApiSystem", // Route name
                "apisystem/{action}/{url}", // URL with parameters
                new
                    {
                        controller = "apisystem",
                        action = "index",
                        id = UrlParameter.Optional
                    } // Parameter defaults
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{*catchall}", // URL with parameters
                new
                    {
                        controller = "home",
                        action = "index"
                    } // Parameter defaults
                );

            routes.LowercaseUrls = true;
        }

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts")
                            .Include(
                                "~/scripts/jquery/jquery.min.js",
                                "~/scripts/highlight/highlight.pack.js",
                                "~/scripts/treeview/jquery.treeview.js",
                                "~/scripts/zeroclipboard/zeroclipboard.js",
                                "~/scripts/scripts.js"));

            bundles.Add(new Bundle("~/content/styles", new CssMinify())
                            .Include(
                                "~/content/hightlight/default.css",
                                "~/content/treeview/jquery.treeview.css",
                                "~/content/styles.css"));
        }

        protected void Application_Start()
        {
            try
            {
                XmlConfigurator.Configure();
                AreaRegistration.RegisterAllAreas();
                RegisterRoutes(RouteTable.Routes);
                RegisterBundles(BundleTable.Bundles);
                ClassNamePluralizer.LoadAndWatch(HttpContext.Current.Server.MapPath("~/App_Data/class_descriptions.xml"));
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Api").Error(error);
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!initialized)
            {
                lock (locker)
                {
                    if (!initialized)
                    {
                        initialized = true;

                        try
                        {
                            //Register cache
                            CacheManifest.AddServerFolder(new HttpContextWrapper(HttpContext.Current), "~/content/img", "*.*");
                            CacheManifest.AddServerFile(new HttpContextWrapper(HttpContext.Current), "~/scripts/modernizr/modernizr-1.7.min.js");
                            CacheManifest.AddCached(new Uri("/", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/basic", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/auth", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/faq", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/filters", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/batch", UriKind.Relative));
                            CacheManifest.AddOnline(new Uri("/portals/search", UriKind.Relative));
                            CacheManifest.AddFallback(new Uri("/portals/search", UriKind.Relative), new Uri("/portals/notfound", UriKind.Relative));

                            LogManager.GetLogger("ASC.Api").Debug("Generate documentations");
                            Documentation.Load(AppDomain.CurrentDomain.RelativeSearchPath);
                            Documentation.GenerateRouteMap("portals");
                        }
                        catch (Exception error)
                        {
                            LogManager.GetLogger("ASC.Api").Error(error);
                        }
                    }
                }
            }
        }
    }
}