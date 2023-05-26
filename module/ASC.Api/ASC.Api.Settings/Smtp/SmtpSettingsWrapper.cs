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


using System.Runtime.Serialization;

namespace ASC.Api.Settings.Smtp
{
    [DataContract(Name = "quota", Namespace = "")]
    public class SmtpSettingsWrapper
    {
        ///<example>mail.example.com</example>
        [DataMember]
        public string Host { get; set; }

        ///<example type="int">25</example>
        [DataMember]
        public int? Port { get; set; }

        ///<example>notify@example.com</example>
        [DataMember]
        public string SenderAddress { get; set; }

        ///<example>Postman</example>
        [DataMember]
        public string SenderDisplayName { get; set; }

        ///<example>notify@example.com</example>
        [DataMember]
        public string CredentialsUserName { get; set; }

        ///<example>{password}</example>
        [DataMember]
        public string CredentialsUserPassword { get; set; }

        ///<example>false</example>
        [DataMember]
        public bool EnableSSL { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool EnableAuth { get; set; }

        [DataMember]
        public bool UseNtlm { get; set; }

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
                UseNtlm = false,
                SenderAddress = "notify@example.com",
                SenderDisplayName = "Postman"
            };
        }
    }
}