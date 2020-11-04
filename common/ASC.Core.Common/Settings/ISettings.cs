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
