/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Runtime.Serialization;

namespace ASC.Web.Core.Files
{
    [Serializable]
    [DataContract]
    public class OnlineEditorsSettings : ISettings
    {
        [DataMember(Name = "NewScheme")]
        public bool NewSchemeSetting { get; set; }

        [DataMember(Name = "RequestedScheme")]
        public bool RequestedSchemeSetting { get; set; }

        public ISettings GetDefault()
        {
            return new OnlineEditorsSettings
                {
                    NewSchemeSetting = false,
                    RequestedSchemeSetting = false
                };
        }

        public Guid ID
        {
            get { return new Guid("{A3ACBFC4-155B-4EA8-8367-BBC586319553}"); }
        }

        public static bool NewScheme
        {
            set
            {
                if (CoreContext.Configuration.Personal)
                {
                    var setting = SettingsManager.Instance.LoadSettingsFor<OnlineEditorsSettings>(SecurityContext.CurrentAccount.ID);
                    setting.NewSchemeSetting = value;
                    setting.RequestedSchemeSetting = true;
                    SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
                }
                else
                {
                    var setting = SettingsManager.Instance.LoadSettings<OnlineEditorsSettings>(TenantProvider.CurrentTenantID);
                    setting.NewSchemeSetting = value;
                    setting.RequestedSchemeSetting = true;
                    SettingsManager.Instance.SaveSettings(setting, TenantProvider.CurrentTenantID);
                }
            }
            get { return NewSchemeFor(SecurityContext.CurrentAccount.ID); }
        }

        public static bool NewSchemeFor(Guid userId)
        {
            return
                SettingsManager.Instance.LoadSettings<OnlineEditorsSettings>(-1).NewSchemeSetting
                || (CoreContext.Configuration.Personal
                        ? SettingsManager.Instance.LoadSettingsFor<OnlineEditorsSettings>(userId).NewSchemeSetting
                        : SettingsManager.Instance.LoadSettings<OnlineEditorsSettings>(TenantProvider.CurrentTenantID).NewSchemeSetting);
        }

        public static bool RequestedScheme
        {
            get
            {
                bool requestedScheme;
                if (CoreContext.Configuration.Personal)
                {
                    var setting = SettingsManager.Instance.LoadSettingsFor<OnlineEditorsSettings>(SecurityContext.CurrentAccount.ID);
                    requestedScheme = setting.RequestedSchemeSetting;
                    setting.RequestedSchemeSetting = true;
                    SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
                }
                else
                {
                    var setting = SettingsManager.Instance.LoadSettings<OnlineEditorsSettings>(TenantProvider.CurrentTenantID);
                    requestedScheme = setting.RequestedSchemeSetting;
                    setting.RequestedSchemeSetting = true;
                    SettingsManager.Instance.SaveSettings(setting, TenantProvider.CurrentTenantID);
                }

                return
                    SettingsManager.Instance.LoadSettings<OnlineEditorsSettings>(-1).RequestedSchemeSetting
                    || requestedScheme;
            }
        }
    }
}