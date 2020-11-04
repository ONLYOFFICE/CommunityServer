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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core.Sms;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.SMS
{
    [Serializable]
    [DataContract]
    public class StudioSmsNotificationSettings : BaseSettings<StudioSmsNotificationSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{2802df61-af0d-40d4-abc5-a8506a5352ff}"); }
        }

        public override ISettings GetDefault()
        {
            return new StudioSmsNotificationSettings { EnableSetting = false, };
        }

        [DataMember(Name = "Enable")]
        public bool EnableSetting { get; set; }


        public static bool Enable
        {
            get { return Load().EnableSetting && SmsProviderManager.Enabled(); }
            set
            {
                var settings = Load();
                settings.EnableSetting = value;
                settings.Save();
            }
        }

        public static bool IsVisibleSettings
        {
            get
            {
                var quota = TenantExtra.GetTenantQuota();
                return CoreContext.Configuration.Standalone
                       || ((!quota.Trial || SetupInfo.SmsTrial)
                           && !quota.NonProfit
                           && !quota.Free
                           && !quota.Open);
            }
        }
    }
}