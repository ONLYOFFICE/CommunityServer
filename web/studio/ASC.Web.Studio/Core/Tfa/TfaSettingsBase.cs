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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.MessagingSystem;

namespace ASC.Web.Studio.Core.TFA
{
    [Serializable]
    [DataContract]
    public abstract class TfaSettingsBase<T> : BaseSettings<T>, ITfaSettings where T : class, ISettings
    {
        [DataMember(Name = "TrustedIps")]
        public List<string> TrustedIps { get; set; }

        [DataMember(Name = "MandatoryUsers")]
        public List<Guid> MandatoryUsers { get; set; }

        [DataMember(Name = "MandatoryGroups")]
        public List<Guid> MandatoryGroups { get; set; }

        [DataMember(Name = "Enable")]
        public bool EnableSetting { get; set; }

        public bool TfaEnabledForUserBase(ITfaSettings settings, Guid userGuid)
        {
            if (!settings.EnableSetting)
            {
                return false;
            }

            if (settings.MandatoryGroups != null && settings.MandatoryGroups.Any())
            {
                var userGroups = CoreContext.UserManager.GetUserGroupsId(userGuid);
                foreach (var group in settings.MandatoryGroups)
                {
                    if (userGroups.Contains(group))
                    {
                        return true;
                    }
                }
            }

            if (settings.MandatoryUsers != null && settings.MandatoryUsers.Any())
            {
                foreach (var user in settings.MandatoryUsers)
                {
                    if (user == userGuid)
                    {
                        return true;
                    }
                }
            }

            if (settings.TrustedIps != null && settings.TrustedIps.Any())
            {
                var requestIps = MessageSettings.GetIPs(HttpContext.Current.Request);

                if (requestIps != null && requestIps.Any(requestIp => settings.TrustedIps.Any(trustedIp => IPSecurity.IPSecurity.MatchIPs(requestIp, trustedIp))))
                {
                    return false;
                }
            }

            return true;
        }
    }
}