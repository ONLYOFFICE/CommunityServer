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

using AjaxPro.Security;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Configuration;
using ASC.Data.Storage.S3;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SearchHandlers;
using log4net.Config;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using TMResourceData;

namespace ASC.Web.Studio
{
    public partial class Startup
    {
        public static void Configure()
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