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

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class IPRestrictionsSettings : BaseSettings<IPRestrictionsSettings>
    {
        ///<example>true</example>
        [DataMember(Name = "Enable")]
        public bool Enable { get; set; }

        public override Guid ID
        {
            get { return new Guid("{2EDDDF64-F792-4498-A638-2E3E6EBB13C9}"); }
        }

        public override ISettings GetDefault()
        {
            return new IPRestrictionsSettings { Enable = false };
        }
    }
}