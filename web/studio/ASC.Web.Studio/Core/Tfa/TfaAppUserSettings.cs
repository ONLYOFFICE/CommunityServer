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