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
using System.Configuration;

namespace ASC.Core.Billing
{
    class TariffSyncServiceSection : ConfigurationSection
    {
        [ConfigurationProperty("period", DefaultValue = "4:0:0")]
        public TimeSpan Period
        {
            get { return (TimeSpan)this["period"]; }
            set { this["period"] = value; }
        }

        [ConfigurationProperty("connectionStringName", DefaultValue = "core")]
        public string ConnectionStringName
        {
            get { return (string)this["connectionStringName"]; }
            set { this["connectionStringName"] = value; }
        }

        public static TariffSyncServiceSection GetSection()
        {
            return (TariffSyncServiceSection)ConfigurationManagerExtension.GetSection("tariffs") ?? new TariffSyncServiceSection();
        }
    }
}
