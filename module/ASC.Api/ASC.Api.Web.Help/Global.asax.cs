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

using System.Web.Optimization;
using ASC.Api.Web.Help.Helpers;
using log4net;
using log4net.Config;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ASC.Api.Web.Help
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly CacheManifest CacheManifest = new CacheManifest();
        private static readonly object locker = new object();
        private static volatile bool initialized = false;

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
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new
                    {
                        controller = "help",
                        action = "index",
                        id = UrlParameter.Optional
                    } // Parameter defaults
                );
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
                            CacheManifest.AddCached(new Uri("/help/faq", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/help/filters", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/help/batch", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/basic", UriKind.Relative));
                            CacheManifest.AddCached(new Uri("/portals/auth", UriKind.Relative));
                            CacheManifest.AddOnline(new Uri("/portals/search", UriKind.Relative));
                            CacheManifest.AddFallback(new Uri("/portals/search", UriKind.Relative), new Uri("/portals/notfound", UriKind.Relative));

                            LogManager.GetLogger("ASC.Api").Debug("Generate documentations");
                            Documentation.Load(AppDomain.CurrentDomain.RelativeSearchPath, HttpContext.Current.Server.MapPath("~/app_data/searchIndex/"));
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