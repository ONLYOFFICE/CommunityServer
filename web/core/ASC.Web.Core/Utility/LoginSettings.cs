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

using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Utility
{
    [Serializable]
    [DataContract]
    public class LoginSettings : BaseSettings<LoginSettings>
    {
        [DataMember(Name = "AttemptCount")]
        public int AttemptCount { get; set; }

        [DataMember(Name = "BlockTime")]
        public int BlockTime { get; set; }

        [DataMember(Name = "CheckPeriod")]
        public int CheckPeriod { get; set; }

        public override Guid ID => new Guid("{588C7E01-8D41-4FCE-9779-D4126E019765}");

        public override ISettings GetDefault()
        {
            return new LoginSettings
            {
                AttemptCount = 5,
                BlockTime = 60,
                CheckPeriod = 60
            };
        }
    }
}
