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
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class PrivacyRoomSettings : BaseSettings<PrivacyRoomSettings>
    {
        [DataMember(Name = "enbaled")]
        public bool EnabledSetting { get; set; }

        public override Guid ID
        {
            get { return new Guid("{FCF002BC-EC4B-4DAB-A6CE-BDE0ABDA44D3}"); }
        }

        public override ISettings GetDefault()
        {
            return new PrivacyRoomSettings
                {
                    EnabledSetting = true
                };
        }

        public static bool Enabled
        {
            get
            {
                return Load().EnabledSetting;
            }
            set
            {
                if (!Available) return;

                var settings = Load();
                settings.EnabledSetting = value;
                settings.Save();
            }
        }

        public static bool Available
        {
            get
            {
                return SetupInfo.IsVisibleSettings(ManagementType.PrivacyRoom.ToString())
                    && CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).PrivacyRoom;
            }
        }
    }
}