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


using ASC.Core;
using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using log4net;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ASC.HealthCheck.Models;
using ASC.HealthCheck.Settings;

namespace ASC.HealthCheck.Controllers
{
    public class ClearCacheApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ClearCacheApiController));
        private const int waitingTimeout = 2000;
        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        [HttpGet]
        public HttpResponseMessage ClearCache()
        {
            try
            {
                var cacheKey = DateTime.UtcNow.ToString("yyyyMMddhhmmss");
                var tasks = new Task<string>[HealthCheckCfgSectionHandler.Instance.WebInstanceCount];

                for (var i = 0; i < tasks.Length; i++)
                {
                    var indexer = i == 0 ? "" : (i + 1).ToString();
                    var serviceName = "monoserve" + indexer;
                    var configPath = string.Format(HealthCheckCfgSectionHandler.Instance.PortalsWebConfigPath, indexer);

                    tasks[i] = Task.Run(() => ClearCacheService(serviceName, configPath, cacheKey));
                }

                Task.WaitAll(tasks, TimeSpan.FromMinutes(1));
                if (tasks[0].Result != string.Empty)
                {
                    return ResultHelper.Error(string.Format(HealthCheckResource.MonoServeRestartError, tasks[0].Result));
                }
                if (tasks[1].Result != string.Empty)
                {
                    return ResultHelper.Error(string.Format(HealthCheckResource.MonoServeRestartError, tasks[1].Result));
                }
                return ResultHelper.Success(HealthCheckResource.CacheCleared);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on ClearCache: {0}, {1}", ex, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.CacheClearError);
            }
        }

        private string ClearCacheService(string serviceName, string configPath, string cacheKey)
        {
            try
            {
                log.DebugFormat("ClearCache: configPath = {0}", configPath);
                ChangeResetCacheKey(configPath, cacheKey);
                if (WorkContext.IsMono)
                {
                    return RestartMonoserve(serviceName);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on ClearCacheService. {0}, {1}",
                    ex, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return HealthCheckResource.ServiceRestartError;
            }
        }

        private string RestartMonoserve(string serviceName)
        {
            var xplatMonoServiceController = new XplatMonoServiceController(serviceName);
            xplatMonoServiceController.StopService();
            // wait till service stopped
            Thread.Sleep(waitingTimeout);
            var monoServeServiceStatus = xplatMonoServiceController.StartService();
            if (monoServeServiceStatus != string.Empty)
            {
                log.ErrorFormat("Unexpected error on ClearCache: monoserve service restart error: {0}", monoServeServiceStatus);
            }
            return monoServeServiceStatus;
        }

        private void ChangeResetCacheKey(string configPath, string cacheKey)
        {
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(
                   new ExeConfigurationFileMap { ExeConfigFilename = configPath }, ConfigurationUserLevel.None);
            if (configuration == null)
            {
                throw new ArgumentException("configuration is null");
            }
            if (configuration.AppSettings == null)
            {
                throw new ArgumentException("configuration.AppSettings is null");
            }
            if (configuration.AppSettings.Settings["web.client.cache.resetkey"] == null)
            {
                throw new ArgumentException("configuration.AppSettings.Settings[\"web.client.cache.resetkey\"] is null");
            }
            configuration.AppSettings.Settings["web.client.cache.resetkey"].Value = cacheKey;
            configuration.Save(ConfigurationSaveMode.Minimal);

            log.DebugFormat("KeyElement web.client.cache.resetkey changed. configPath = {0}", configPath);
        }
    }
}