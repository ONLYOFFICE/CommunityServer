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


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Configuration;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System.Globalization;

namespace ASC.Web.Files.Classes
{
    [Serializable]
    [DataContract]
    public class FilesSettings : ISettings
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        [DataMember(Name = "EnableThirdpartySettings")]
        public bool EnableThirdpartySetting { get; set; }

        [DataMember(Name = "StoreOriginalFiles")]
        public bool StoreOriginalFilesSetting { get; set; }

        [DataMember(Name = "UpdateIfExist")]
        public bool UpdateIfExistSetting { get; set; }

        [DataMember(Name = "ExternalIP")]
        public KeyValuePair<bool, string> CheckExternalIPSetting { get; set; }

        [DataMember(Name = "ConvertNotify")]
        public bool ConvertNotifySetting { get; set; }

        public ISettings GetDefault()
        {
            return new FilesSettings
                {
                    EnableThirdpartySetting = true,
                    StoreOriginalFilesSetting = true,
                    UpdateIfExistSetting = false,
                    CheckExternalIPSetting = new KeyValuePair<bool, string>(true, DateTime.MinValue.ToString(CultureInfo)),
                    ConvertNotifySetting = true,
                };
        }

        public Guid ID
        {
            get { return new Guid("{03B382BD-3C20-4f03-8AB9-5A33F016316E}"); }
        }

        public static bool EnableThirdParty
        {
            set
            {
                var setting = new FilesSettings
                    {
                        EnableThirdpartySetting = value
                    };
                SettingsManager.Instance.SaveSettings(setting, TenantProvider.CurrentTenantID);
            }
            get { return SettingsManager.Instance.LoadSettings<FilesSettings>(TenantProvider.CurrentTenantID).EnableThirdpartySetting; }
        }

        public static bool StoreOriginalFiles
        {
            set
            {
                var setting = SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID);
                setting.StoreOriginalFilesSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get { return SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID).StoreOriginalFilesSetting; }
        }

        public static bool UpdateIfExist
        {
            set
            {
                var setting = SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID);
                setting.UpdateIfExistSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get { return SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID).UpdateIfExistSetting; }
        }

        public static KeyValuePair<bool, DateTime> CheckHaveExternalIP
        {
            set
            {
                var setting = new FilesSettings
                    {
                        CheckExternalIPSetting = new KeyValuePair<bool, string>(value.Key, DateTime.UtcNow.ToString(CultureInfo))
                    };
                SettingsManager.Instance.SaveSettings(setting, -1);
            }
            get
            {
                var pair = SettingsManager.Instance.LoadSettings<FilesSettings>(-1).CheckExternalIPSetting;
                return new KeyValuePair<bool, DateTime>(pair.Key, Convert.ToDateTime(pair.Value, CultureInfo));
            }
        }

        public static bool ConvertNotify
        {
            set
            {
                var setting = SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID);
                setting.ConvertNotifySetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get { return SettingsManager.Instance.LoadSettingsFor<FilesSettings>(SecurityContext.CurrentAccount.ID).ConvertNotifySetting; }
        }
    }
}