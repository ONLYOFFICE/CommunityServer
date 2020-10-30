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

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "DNSInfo")]
    public class ServerDomainDnsData
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        [DataMember(IsRequired = true)]
        public ServerDomainMxRecordData MxRecord { get; set; }

        [DataMember(IsRequired = true)]
        public ServerDomainDnsRecordData SpfRecord { get; set; }

        [DataMember(IsRequired = true)]
        public ServerDomainDkimRecordData DkimRecord { get; set; }

        [DataMember(IsRequired = true)]
        public ServerDomainDnsRecordData DomainCheckRecord { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsVerified
        {
            get
            {
                return MxRecord.IsVerified &&
                       SpfRecord.IsVerified &&
                       DkimRecord.IsVerified;
            }
        }
    }
}
