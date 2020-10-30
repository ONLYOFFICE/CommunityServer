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


using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Core.Users;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "mention", Namespace = "")]
    public class MentionWrapper
    {
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email
        {
            get { return User.Email; }
            set { }
        }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id
        {
            get { return User.ID.ToString(); }
            set { }
        }

        [DataMember(Name = "hasAccess", EmitDefaultValue = false)]
        public bool HasAccess { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name
        {
            get { return User.DisplayUserName(false); }
            set { }
        }

        public UserInfo User;

        public MentionWrapper(UserInfo user)
        {
            User = user;
        }
    }


    [DataContract(Name = "message", Namespace = "")]
    public class MentionMessageWrapper
    {
        [DataMember(Name = "actionLink")]
        public DocumentService.Configuration.EditorConfiguration.ActionLinkConfig ActionLink { get; set; }

        [DataMember(Name = "emails")]
        public List<string> Emails { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}