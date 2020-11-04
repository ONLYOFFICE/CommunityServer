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
using ASC.Core;
using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class PersonalSettings : BaseSettings<PersonalSettings>
    {
        [DataMember(Name = "IsNewUser")]
        public bool IsNewUserSetting { get; set; }

        [DataMember(Name = "IsNotActivated")]
        public bool IsNotActivatedSetting { get; set; }

        public override Guid ID
        {
            get { return new Guid("{B3427865-8E32-4E66-B6F3-91C61922239F}"); }
        }

        public override ISettings GetDefault()
        {
            return new PersonalSettings
                {
                    IsNewUserSetting = false,
                    IsNotActivatedSetting = false,
                };
        }

        public static bool IsNewUser
        {
            get { return LoadForCurrentUser().IsNewUserSetting; }
            set
            {
                var settings = LoadForCurrentUser();
                settings.IsNewUserSetting = value;
                settings.SaveForCurrentUser();
            }
        }

        public static bool IsNotActivated
        {
            get { return LoadForCurrentUser().IsNotActivatedSetting; }
            set
            {
                var settings = LoadForCurrentUser();
                settings.IsNotActivatedSetting = value;
                settings.SaveForCurrentUser();
            }
        }
    }
}