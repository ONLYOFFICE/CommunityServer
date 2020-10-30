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
using ASC.Core.Common.Settings;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract]
    public class ServerNotificationAddressSettings : BaseSettings<ServerNotificationAddressSettings>
    {
        [DataMember]
        public string NotificationAddress { get; set; }

        public override ISettings GetDefault()
        {
            return new ServerNotificationAddressSettings {NotificationAddress = string.Empty};
        }

        public override Guid ID
        {
            get { return new Guid("{C440A7BE-A336-4071-A57E-5E89725E1CF8}"); }
        }
    }
}
