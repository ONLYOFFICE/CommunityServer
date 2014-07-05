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
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ASC.Common.Data;
using ASC.Web.Files.Api;
using Resources;
using TMResourceData;

namespace ASC.Web.Mobile
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("products/{*pathInfo}");

            routes.MapRoute(
                "mc.manifest", // Route name
                "mc.manifest", // URL with parameters
                new { controller = "Manifest", action = "Index" }
            );

            routes.MapRoute(
                "resources.js", // Route name
                "scripts/{culture}/resources.js", // URL with parameters
                new { controller = "Resources", action = "Index" }
            );

            routes.MapRoute(
                "FileViewer", // Route name
                "fileview/{folderid}/{id}/{ver}/{title}", // URL with parameters
                new { controller = "FileViewer", action = "Index", title = UrlParameter.Optional, ver = UrlParameter.Optional }
            );

            routes.MapRoute(
                "Login", // Route name
                "sign-in", // URL with parameters
                new { controller = "Account", action = "SignIn" } // Parameter defaults
            );

            routes.MapRoute(
              "Federated", // Route name
              "callback", // URL with parameters
              new { controller = "Account", action = "Federated" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);

            var iisversion = GetIISVersion();
            if (iisversion != 0 && iisversion < 7)
            {
                AddMimeMapping(".png", "image/png");
                AddMimeMapping(".svg", "image/svg+xml");
            }

            DbRegistry.Configure();

            if (ConfigurationManager.AppSettings["resources.from-db"] == "true")
            {
                AssemblyWork.UploadResourceData(AppDomain.CurrentDomain.GetAssemblies());
                AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => AssemblyWork.UploadResourceData(AppDomain.CurrentDomain.GetAssemblies());
            }

            //HACK: Register CRM and Projects file integrations
            if (!FilesIntegration.IsRegisteredFileSecurityProvider("projects", "project")) 
                FilesIntegration.RegisterFileSecurityProvider("projects", "project", new ASC.Projects.Engine.SecurityAdapterProvider());

            if (!FilesIntegration.IsRegisteredFileSecurityProvider("crm", "crm_common"))
                FilesIntegration.RegisterFileSecurityProvider("crm", "crm_common", new ASC.CRM.Core.FileSecurityProvider());

        }

        private void AddMimeMapping(string extension, string mimeType)
        {
            try
            {
                var mimeMappingType = Type.GetType("System.Web.MimeMapping, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                if (mimeMappingType != null)
                {
                    var addMimeMapping = mimeMappingType.GetMethod("AddMimeMapping", BindingFlags.Static | BindingFlags.NonPublic);
                    if (addMimeMapping != null)
                    {
                        addMimeMapping.Invoke(null, new object[] { extension, mimeType });
                    }
                }
            }
            catch { }
        }

        private int GetIISVersion()
        {
            try
            {
                var version = HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"];
                if (!string.IsNullOrEmpty(version) && version.Contains("/"))
                {
                    version = version.Split('/')[1];
                    if (version.Contains("."))
                    {
                        version = version.Split('.')[0];
                    }
                    return int.Parse(version);
                }
            }
            catch { }
            return 0;
        }
    }
}
