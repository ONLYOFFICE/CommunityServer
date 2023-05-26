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
using System.Runtime.Serialization;

using ASC.Mail.Enums;
using ASC.Mail.Enums.Filter;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "FilterOptions")]
    public class MailSieveFilterOptionsData
    {
        public MailSieveFilterOptionsData()
        {
            ApplyTo = new MailSieveFilterOptionsApplyToData();
        }

        ///<example name="matchMultiConditions" type="int">1</example>
        [DataMember(Name = "matchMultiConditions")]
        public MatchMultiConditionsType MatchMultiConditions { get; set; }

        ///<type name="applyTo">ASC.Mail.Data.Contracts.MailSieveFilterOptionsApplyToData, ASC.Mail</type>
        [DataMember(Name = "applyTo")]
        public MailSieveFilterOptionsApplyToData ApplyTo { get; set; }

        ///<example name="ignoreOther">true</example>
        [DataMember(Name = "ignoreOther")]
        public bool IgnoreOther { get; set; }
    }

    [Serializable]
    [DataContract(Namespace = "", Name = "FilterOptionsApplyTo")]
    public class MailSieveFilterOptionsApplyToData
    {
        public MailSieveFilterOptionsApplyToData()
        {
            Folders = new[] { (int)FolderType.Inbox };
            Mailboxes = new int[] { };
        }
        ///<example name="folders" type="int">1</example>
        ///<collection>list</collection>
        [DataMember(Name = "folders")]
        public int[] Folders { get; set; }

        ///<example name="mailboxes" type="int">1</example>
        ///<collection>list</collection>
        [DataMember(Name = "mailboxes")]
        public int[] Mailboxes { get; set; }

        ///<example name="withAttachments" type="int">1</example>
        [DataMember(Name = "withAttachments")]
        public ApplyToAttachmentsType WithAttachments { get; set; }
    }
}
