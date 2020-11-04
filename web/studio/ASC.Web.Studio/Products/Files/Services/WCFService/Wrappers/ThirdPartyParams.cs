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


using System.Runtime.Serialization;
using ASC.Files.Core;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "third_party", Namespace = "")]
    public class ThirdPartyParams
    {
        [DataMember(Name = "auth_data", EmitDefaultValue = false)]
        public AuthData AuthData { get; set; }

        [DataMember(Name = "corporate")]
        public bool Corporate { get; set; }

        [DataMember(Name = "customer_title")]
        public string CustomerTitle { get; set; }

        [DataMember(Name = "provider_id")]
        public string ProviderId { get; set; }

        [DataMember(Name = "provider_key")]
        public string ProviderKey { get; set; }
    }
}