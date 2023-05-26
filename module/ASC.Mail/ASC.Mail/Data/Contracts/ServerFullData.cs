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
    [DataContract(Namespace = "", Name = "FullServer")]
    public class ServerFullData
    {
        ///<type>ASC.Mail.Data.Contracts.ServerData, ASC.Mail</type>
        [DataMember(IsRequired = true)]
        public ServerData Server { get; set; }

        ///<type>ASC.Mail.Data.Contracts.ServerDomainData, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true)]
        public List<ServerDomainData> Domains { get; set; }

        ///<type>ASC.Mail.Data.Contracts.ServerMailboxData, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true)]
        public List<ServerMailboxData> Mailboxes { get; set; }

        ///<type>ASC.Mail.Data.Contracts.ServerDomainGroupData, ASC.Mail</type>
        ///<collection>list</collection>
        [DataMember(IsRequired = true)]
        public List<ServerDomainGroupData> Mailgroups { get; set; }

    }
}
