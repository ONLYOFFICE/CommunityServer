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
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Mailbox")]
    public class ServerMailboxData
    {
        ///<example type="int">1234</example>
        [DataMember(IsRequired = true)]
        public int Id { get; set; }

        ///<example>UserId</example>
        [DataMember(IsRequired = true)]
        public string UserId { get; set; }

        ///<example>UserDisplayName</example>
        [DataMember(IsRequired = true)]
        public string UserDisplayName { get; set; }

        ///<type>ASC.Mail.Data.Contracts.ServerDomainAddressData, ASC.Mail</type>
        [DataMember(IsRequired = true)]
        public ServerDomainAddressData Address { get; set; }

        ///<example>Name</example>
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        ///<type>ASC.Mail.Data.Contracts.ServerDomainAddressData, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true)]
        public List<ServerDomainAddressData> Aliases { get; set; }
    }
}
