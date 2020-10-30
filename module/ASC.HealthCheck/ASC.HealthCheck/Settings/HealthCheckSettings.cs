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

namespace ASC.HealthCheck.Settings
{
    [Serializable]
    public class HealthCheckSettings
    {
        public HealthCheckSettings()
        {
            Emails = new List<string>();
            PhoneNumbers = new List<string>();
        }

        [DataMember]
        public int FakeTenantId { get; set; }

        [DataMember]
        public bool SendNotify { get; set; }

        [DataMember]
        public bool SendEmail { get; set; }

        [DataMember]
        public bool SendSms { get; set; }

        [DataMember]
        public List<string> Emails { get; set; }

        [DataMember]
        public List<string> PhoneNumbers { get; set; }

        [DataMember]
        public string SmsOperatorUrlClickatel { get; set; }

        [DataMember]
        public string SmsOperatorUrlClickatelUSA { get; set; }

        [DataMember]
        public string SmsOperatorUrlSmsc { get; set; }
    }
}
