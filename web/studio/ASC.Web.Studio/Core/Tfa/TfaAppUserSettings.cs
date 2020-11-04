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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.TFA
{
    [Serializable]
    [DataContract]
    public class TfaAppUserSettings : BaseSettings<TfaAppUserSettings>
    {
        [DataMember(Name = "BackupCodes")]
        public IEnumerable<BackupCode> CodesSetting { get; set; }

        [DataMember(Name = "Salt")]
        public long SaltSetting { get; set; }

        public override Guid ID
        {
            get { return new Guid("{EAF10611-BE1E-4634-B7A1-57F913042F78}"); }
        }

        public override ISettings GetDefault()
        {
            return new TfaAppUserSettings
                {
                    CodesSetting = new List<BackupCode>(),
                    SaltSetting = 0
                };
        }

        public static long GetSalt(Guid userId)
        {
            var settings = LoadForUser(userId);
            var salt = settings.SaltSetting;
            if (salt == 0)
            {
                var from = new DateTime(2018, 07, 07, 0, 0, 0, DateTimeKind.Utc);
                settings.SaltSetting = salt = (long)(DateTime.UtcNow - from).TotalMilliseconds;

                settings.SaveForUser(userId);
            }
            return salt;
        }

        public static IEnumerable<BackupCode> BackupCodes
        {
            get { return LoadForCurrentUser().CodesSetting; }
            set
            {
                var settings = LoadForCurrentUser();
                settings.CodesSetting = value;
                settings.SaveForCurrentUser();
            }
        }

        public static IEnumerable<BackupCode> BackupCodesForUser(Guid userId)
        {
            return LoadForUser(userId).CodesSetting;
        }

        public static void DisableCodeForUser(Guid userId, string code)
        {
            var settings = LoadForUser(userId);
            var query = settings.CodesSetting.Where(x => x.Code == code).ToList();

            if (query.Any())
                query.First().IsUsed = true;

            settings.SaveForUser(userId);
        }

        public static bool EnableForUser(Guid guid)
        {
            return LoadForUser(guid).CodesSetting.Any();
        }

        public static void DisableForUser(Guid guid)
        {
            var defaultSettings = new TfaAppUserSettings().GetDefault() as TfaAppUserSettings;
            if (defaultSettings != null)
            {
                defaultSettings.SaveForUser(guid);
            }
        }

        public static bool IsVisibleSettings
        {
            get
            {
                var quota = TenantExtra.GetTenantQuota();
                return CoreContext.Configuration.Standalone
                       || (!quota.Trial
                           && !quota.NonProfit
                           && !quota.Free
                           && !quota.Open);
            }
        }
    }
}