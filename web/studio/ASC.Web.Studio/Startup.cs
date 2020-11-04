/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

using AjaxPro.Security;

using ASC.ActiveDirectory.Base;
using ASC.Common.Data;
using ASC.Common.Logging;
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

using RedisSessionProvider.Config;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.LegacyConfiguration;

using TMResourceData;

using WhiteLabelHelper = ASC.Web.Studio.Utility.WhiteLabelHelper;

namespace ASC.Web.Studio
{
    public partial class Startup
    {
        public static void Configure()
        {
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

            LdapNotifyHelper.RegisterAll();

            try
            {
                new S3UploadGuard().DeleteExpiredUploadsAsync(TimeSpan.FromDays(1));//todo:
            }
            catch (Exception)
            {
            }

            try
            {
                Core.WarmUp.Instance.Start();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error("Start Warmup", ex);
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
            var configuration = ConfigurationManagerExtension.GetSection("redisCacheClient") as RedisCachingSectionHandler;
            if (configuration != null)
            {
                RedisConnectionConfig.GetSERedisServerConfig = context =>
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