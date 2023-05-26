/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Core.Common.Settings;


namespace ASC.Core.Tenants
{
    [Serializable]
    [DataContract]
    public class TenantUserQuotaSettings : BaseSettings<TenantUserQuotaSettings>
    {
        [DataMember]
        public bool EnableUserQuota { get; set; }
        [DataMember]
        public long DefaultUserQuota { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime LastRecalculateDate { get; set; }

        public override ISettings GetDefault()
        {
            return new TenantUserQuotaSettings
            {
                EnableUserQuota = false,
                DefaultUserQuota = -1,
                LastRecalculateDate = new DateTime(0L, DateTimeKind.Unspecified)
            };
        }

        
        public override Guid ID
        {
            get { return new Guid("{5FE28053-BCD4-466B-8A4B-71B612F0D6FC}"); }
        }
    }
}
