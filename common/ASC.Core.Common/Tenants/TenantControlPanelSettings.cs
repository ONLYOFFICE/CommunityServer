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
using ASC.Core.Common.Settings;

namespace ASC.Core.Tenants
{
    [Serializable]
    [DataContract]
    public class TenantControlPanelSettings : BaseSettings<TenantControlPanelSettings>
    {
        [DataMember(Name = "LimitedAccess")]
        public bool LimitedAccess { get; set; }

        public override Guid ID
        {
            get { return new Guid("{880585C4-52CD-4AE2-8DA4-3B8E2772753B}"); }
        }

        public override ISettings GetDefault()
        {
            return new TenantControlPanelSettings
            {
                LimitedAccess = false
            };
        }

        public static TenantControlPanelSettings Instance
        {
            get
            {
                return Load();
            }
        }
    }
}