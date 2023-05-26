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


using System.Runtime.Serialization;

using ASC.Files.Core;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "third_party", Namespace = "")]
    public class ThirdPartyParams
    {
        ///<type name="auth_data">ASC.Files.Core.AuthData, ASC.Web.Files</type>
        [DataMember(Name = "auth_data", EmitDefaultValue = false)]
        public AuthData AuthData { get; set; }

        ///<example name="corporate">false</example>
        [DataMember(Name = "corporate")]
        public bool Corporate { get; set; }

        ///<example name="customer_title">customer_title</example>
        [DataMember(Name = "customer_title")]
        public string CustomerTitle { get; set; }

        ///<example name="provider_id">provider_id</example>
        [DataMember(Name = "provider_id")]
        public string ProviderId { get; set; }

        ///<example name="provider_key">provider_key</example>
        [DataMember(Name = "provider_key")]
        public string ProviderKey { get; set; }
    }
}