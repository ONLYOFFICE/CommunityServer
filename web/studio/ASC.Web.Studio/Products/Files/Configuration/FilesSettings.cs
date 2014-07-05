/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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