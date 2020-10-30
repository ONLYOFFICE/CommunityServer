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

namespace ASC.Api.Settings.Smtp
{
    [DataContract(Name = "quota", Namespace = "")]
    public class SmtpSettingsWrapper
    {
        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int? Port { get; set; }

        [DataMember]
        public string SenderAddress { get; set; }

        [DataMember]
        public string SenderDisplayName { get; set; }

        [DataMember]
        public string CredentialsUserName { get; set; }

        [DataMember]
        public string CredentialsUserPassword { get; set; }

        [DataMember]
        public bool EnableSSL { get; set; }

        [DataMember]
        public bool EnableAuth { get; set; }

        public static SmtpSettingsWrapper GetSample()
        {
            return new SmtpSettingsWrapper
                {
                    Host = "mail.example.com",
                    Port = 25,
                    CredentialsUserName = "notify@example.com",
                    CredentialsUserPassword = "{password}",
                    EnableAuth = true,
                    EnableSSL = false,
                    SenderAddress = "notify@example.com",
                    SenderDisplayName = "Postman"
                };
        }
    }
}