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
using log4net;
using Newtonsoft.Json;
using System;

namespace ASC.HealthCheck.Settings
{
    public class HealthCheckSettingsAccessor
    {
        public static HealthCheckSettings GetHealthCheckSettings()
        {
            try
            {
                var healthcheckSettingsString = CoreContext.Configuration.GetSetting("HealthCheckSettings");
                return !string.IsNullOrWhiteSpace(healthcheckSettingsString) ? 
                    JsonConvert.DeserializeObject<HealthCheckSettings>(healthcheckSettingsString) : 
                    new HealthCheckSettings();
            }
            catch(Exception ex)
            {
                LogManager.GetLogger(typeof(HealthCheckSettingsAccessor)).ErrorFormat("GetHealthCheckSettings. Error! {0}, {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : string.Empty);
                return new HealthCheckSettings();
            }
        }

        public static void SaveHealthCheckSettings(HealthCheckSettings healthcheckSettings)
        {
            try
            {
                CoreContext.Configuration.SaveSetting("HealthCheckSettings", JsonConvert.SerializeObject(healthcheckSettings));
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(HealthCheckSettingsAccessor)).ErrorFormat("SaveHealthCheckSettings. Error! {0}, {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : string.Empty);
            }
        }
    }
}
