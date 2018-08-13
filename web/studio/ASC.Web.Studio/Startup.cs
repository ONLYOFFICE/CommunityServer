/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

using ASC.Common.Data;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Data.Storage.S3;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Security;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SearchHandlers;
using ASC.Web.Studio.Utility;
using TMResourceData;

using AjaxPro.Security;
using log4net.Config;
using RedisSessionProvider.Config;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;

using WhiteLabelHelper = ASC.Web.Studio.Utility.WhiteLabelHelper;

namespace ASC.Web.Studio
{
    public partial class Startup
    {
        public static void Configure()
        {
            XmlConfigurator.Configure();

            DbRegistry.Configure();

            PrepareRedisSessionProvider();

            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                var url = HttpContext.Current.Request.GetUrlRewriter();
                CommonLinkUtility.Initialize(new UriBuilder(url.Scheme, url.Host, url.Port).Uri.ToString());
            }

            ConfigureWebApi();

            if (DBResourceManager.ResourcesFromDataBase)
            {
                DBResourceManager.WhiteLableEnabled = true;
                DBResourceManager.PatchAssemblies();
            }

            AjaxSecurityChecker.Instance.CheckMethodPermissions += AjaxCheckMethodPermissions;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            //try
            //{
            //    AmiPublicDnsSyncService.Synchronize();
            //}
            //catch { }

            NotifyConfiguration.Configure();

            WebItemManager.Instance.LoadItems();

            SearchHandlerManager.Registry(new StudioSearchHandler());

            StorageFactory.InitializeHttpHandlers();

            BundleConfig.Configure();

            WhiteLabelHelper.ApplyPartnerWhiteLableSettings();

            try
            {
                (new S3UploadGuard()).DeleteExpiredUploads(TimeSpan.FromDays(1));//todo:
            }
            catch (Exception)
            {
            }
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

        private static void PrepareRedisSessionProvider()
        {
            var configuration = RedisCachingSectionHandler.GetConfig();
            if (configuration != null)
            {
                RedisConnectionConfig.GetSERedisServerConfig = (HttpContextBase context) =>
                {
                    if (configuration.RedisHosts != null && configuration.RedisHosts.Count > 0)
                    {
                        var host = configuration.RedisHosts[0];
                        return new KeyValuePair<string, ConfigurationOptions>("DefaultConnection",
                            ConfigurationOptions.Parse(String.Concat(host.Host, ":", host.CachePort)));
                    }
                    return new KeyValuePair<string, ConfigurationOptions>();
                };
            }
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            const string restSharpName = "RestSharp";
            var fullyQualifiedName = args.Name;
            if (!fullyQualifiedName.Contains(restSharpName)) return null;

            try
            {
                var restSharpFilePath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin", restSharpName + ".dll");
                return Assembly.LoadFrom(restSharpFilePath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}