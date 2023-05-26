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

using ASC.AuditTrail;
using ASC.MessagingSystem;
using ASC.Specific;

namespace ASC.Api.Security
{
    public class LoginEventWrapper
    {
        public LoginEventWrapper(LoginEvent loginEvent)
        {
            Id = loginEvent.Id;
            Date = (ApiDateTime)loginEvent.Date;
            User = loginEvent.UserName;
            UserId = loginEvent.UserId;
            Login = loginEvent.Login;
            Action = loginEvent.ActionText;
            ActionId = (MessageAction)loginEvent.Action;
            IP = loginEvent.IP;
            Browser = loginEvent.Browser;
            Platform = loginEvent.Platform;
            Page = loginEvent.Page;
        }

        public int Id { get; set; }

        public ApiDateTime Date { get; set; }

        public string User { get; set; }

        public Guid UserId { get; set; }
        public string Login { get; set; }

        public string Action { get; set; }

        public MessageAction ActionId { get; set; }

        public string IP { get; set; }

        public string Browser { get; set; }

        public string Platform { get; set; }

        public string Page { get; set; }
    }
}