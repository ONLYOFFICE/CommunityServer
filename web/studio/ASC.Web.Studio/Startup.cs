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


using AjaxPro.Security;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Configuration;
using ASC.Data.Storage;
using ASC.Data.Storage.S3;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SearchHandlers;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using TMResourceData;

namespace ASC.Web.Studio
{
    public partial class Startup
    {
        public static void Configure(HttpApplication application)
        {
            XmlConfigurator.Configure();
            
            DbRegistry.Configure();

            ConfigureWebApi();

            if (ConfigurationManager.AppSettings["resources.from-db"] == "true")
            {
                AssemblyWork.UploadResourceData(AppDomain.CurrentDomain.GetAssemblies());
                AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => AssemblyWork.UploadResourceData(AppDomain.CurrentDomain.GetAssemblies());
            }

            AjaxSecurityChecker.Instance.CheckMethodPermissions += AjaxCheckMethodPermissions;
            
            try
            {
                AmiPublicDnsSyncService.Synchronize();
            }
            catch { }
            
            NotifyConfiguration.Configure();
            
            WebItemManager.Instance.LoadItems();
            
            SearchHandlerManager.Registry(new StudioSearchHandler());

            StorageFactory.InitializeHttpHandlers();
            (new S3UploadGuard()).DeleteExpiredUploads(TimeSpan.FromDays(1));
            
            BundleConfig.Configure();
        }


        private static bool AjaxCheckMethodPermissions(MethodInfo method)
        {
            var authorized = SecurityContext.IsAuthenticated;
            if (!authorized && HttpContext.Current != null)
            {
                authorized = method.GetCustomAttributes(typeof(SecurityAttribute), true)
                                   .Cast<SecurityAttribute>()
                                   .Any(a => a.CheckAuthorization(HttpContext.Current));
                if (!authorized)
                {
                    authorized = Global.Authenticate();
                }
            }
            return authorized;
        }
    }
}