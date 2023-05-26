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


using System.Collections.Generic;
using System.Runtime.Serialization;

using ASC.Web.Studio.UserControls.Users.UserProfile;

namespace ASC.Specific.CapabilitiesApi
{
    [DataContract(Name = "capabilities", Namespace = "")]
    public class CapabilitiesData
    {
        /// <example>false</example>
        /// <collection>list</collection>
        [DataMember]
        public bool LdapEnabled { get; set; }

        /// <example>google,facebook,twitter,linkedin,mailru,vk,yandex,gosuslugi</example>
        /// <collection split=",">list</collection>
        [DataMember]
        public bool OauthEnabled { get; set; }

        [DataMember]
        public List<string> Providers { get; set; }

        /// <example></example>
        [DataMember]
        public string SsoLabel { get; set; }

        /// <summary>
        /// if empty sso is disabled
        /// </summary>
        /// <example></example>
        [DataMember]
        public string SsoUrl { get; set; }

        public static CapabilitiesData GetSample()
        {
            return new CapabilitiesData
            {
                LdapEnabled = false,
                Providers = AccountLinkControl.AuthProviders,
                SsoLabel = string.Empty,
                SsoUrl = string.Empty,
            };
        }
    }
}

