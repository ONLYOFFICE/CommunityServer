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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Filter")]
    public class MailSieveFilterData
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "conditions")]
        public List<MailSieveFilterConditionData> Conditions { get; set; }

        [DataMember(Name = "actions")]
        public List<MailSieveFilterActionData> Actions { get; set; }

        [DataMember(Name = "options")]
        public MailSieveFilterOptionsData Options { get; set; }

        public MailSieveFilterData()
        {
            Actions = new List<MailSieveFilterActionData>();
            Conditions = new List<MailSieveFilterConditionData>();
            Options = new MailSieveFilterOptionsData();
        }
    }
}
