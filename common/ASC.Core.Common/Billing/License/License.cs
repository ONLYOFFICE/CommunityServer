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
using System.Diagnostics;
using System.Runtime.Serialization;

using Newtonsoft.Json.Linq;

namespace ASC.Core.Billing
{
    [Serializable]
    [DataContract(Name = "license", Namespace = "")]
    [DebuggerDisplay("{DueDate}")]
    public class License
    {
        public string OriginalLicense { get; set; }


        [DataMember(Name = "customization")]
        public bool Customization { get; set; }

        [DataMember(Name = "end_date")]
        public DateTime DueDate { get; set; }

        [DataMember(Name = "trial")]
        public bool Trial { get; set; }

        [DataMember(Name = "customer_id")]
        public string CustomerId { get; set; }

        [DataMember(Name = "users_count")]
        public int DSUsersCount { get; set; }

        [DataMember(Name = "users_expire")]
        public int DSUsersExpire { get; set; }

        [DataMember(Name = "connections")]
        public int DSConnections { get; set; }

        [DataMember(Name = "signature")]
        public string Signature { get; set; }


        public static License Parse(string licenseString)
        {
            if (string.IsNullOrEmpty(licenseString)) throw new BillingNotFoundException("License file is empty");

            var licenseJson = JObject.Parse(licenseString);
            if (licenseJson == null) throw new BillingNotFoundException("Can't parse license");

            var license = licenseJson.ToObject<License>();
            license.OriginalLicense = licenseString;

            return license;
        }
    }
}