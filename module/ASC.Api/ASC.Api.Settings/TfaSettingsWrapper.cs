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

using ASC.Web.Studio.Core.TFA;

namespace ASC.Api.Settings
{
    [DataContract(Name = "tfaSettings", Namespace = "")]
    public class TfaSettingsWrapper
    {
        [DataMember]
        public ITfaSettings TfaSettings;

        [DataMember]
        public TfaSettingsType TfaSettingsType;

        public static TfaSettingsWrapper GetSample()
        {
            return new TfaSettingsWrapper
            {
                TfaSettings = new TfaAppAuthSettings
                {
                    EnableSetting = true,
                    MandatoryUsers = new List<Guid>(),
                    MandatoryGroups = new List<Guid>(),
                    TrustedIps = new List<string>()
                },
                TfaSettingsType = TfaSettingsType.App
            };
        }
    }

    public enum TfaSettingsType
    {
        None,
        Sms,
        App
    }
}
