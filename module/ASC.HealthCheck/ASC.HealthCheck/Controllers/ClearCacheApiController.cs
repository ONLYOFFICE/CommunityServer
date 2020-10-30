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