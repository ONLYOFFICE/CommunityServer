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
using System.Runtime.Serialization;
using ASC.Core.Tenants;

namespace ASC.Core.Common.Settings
{
    public interface ISettings
    {
        Guid ID { get; }
        ISettings GetDefault();
    }

    [Serializable]
    [DataContract]
    public abstract class BaseSettings<T> : ISettings where T: class, ISettings
    {
        private static int TenantID
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static Guid CurrentUserID
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }

        private static SettingsManager SettingsManagerInstance
        {
            get { return SettingsManager.Instance; }
        }

        public static T Load()
        {
            return SettingsManagerInstance.LoadSettings<T>(TenantID);
        }

        public static T LoadForCurrentUser()
        {
            return LoadForUser(CurrentUserID);
        }

        public static T LoadForUser(Guid userId)
        {
            return SettingsManagerInstance.LoadSettingsFor<T>(TenantID, userId);
        }

        public static T LoadForDefaultTenant()
        {
            return LoadForTenant(Tenant.DEFAULT_TENANT);
        }

        public static T LoadForTenant(int tenantId)
        {
            return SettingsManagerInstance.LoadSettings<T>(tenantId);
        }

        public virtual bool Save()
        {
            return SettingsManagerInstance.SaveSettings(this, TenantID);
        }

        public bool SaveForCurrentUser()
        {
            return SaveForUser(CurrentUserID);
        }

        public bool SaveForUser(Guid userId)
        {
            return SettingsManagerInstance.SaveSettingsFor(this, userId);
        }

        public bool SaveForDefaultTenant()
        {
            return SaveForTenant(Tenant.DEFAULT_TENANT);
        }

        public bool SaveForTenant(int tenantId)
        {
            return SettingsManagerInstance.SaveSettings(this, tenantId);
        }

        public void ClearCache()
        {
            SettingsManagerInstance.ClearCache<T>();
        }

        public abstract Guid ID { get; }

        public abstract ISettings GetDefault();
    }
}
