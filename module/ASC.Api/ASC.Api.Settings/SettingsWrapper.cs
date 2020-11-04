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
using System.Runtime.Serialization;
using ASC.Core.Tenants;

namespace ASC.Api.Settings
{
    [DataContract(Name = "settings", Namespace = "")]
    public class SettingsWrapper
    {
        [DataMember]
        public string Timezone { get; set; }

        [DataMember]
        public List<string> TrustedDomains { get; set; }

        [DataMember]
        public TenantTrustedDomainsType TrustedDomainsType { get; set; }

        [DataMember]
        public string Culture { get; set; }

        [DataMember]
        public TimeSpan UtcOffset { get; set; }


        [DataMember]
        public double UtcHoursOffset { get; set; }

        public static SettingsWrapper GetSample()
        {
            return new SettingsWrapper
                {
                    Culture = "en-US",
                    Timezone = TimeZoneInfo.Utc.ToString(),
                    TrustedDomains = new List<string> {"mydomain.com"},
                    UtcHoursOffset = -8.5,
                    UtcOffset = TimeSpan.FromHours(-8.5)
                };
        }
    }
}