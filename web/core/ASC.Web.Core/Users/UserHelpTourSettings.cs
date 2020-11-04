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
using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Users
{
    [Serializable]
    [DataContract]
    public class UserHelpTourSettings : BaseSettings<UserHelpTourSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{DF4B94B7-42C8-4fce-AAE2-D479F3B39BDD}"); }
        }

        [DataMember(Name = "ModuleHelpTour")]
        public Dictionary<Guid, int> ModuleHelpTour { get; set; }

        [DataMember(Name = "IsNewUser")]
        public bool IsNewUser { get; set; }

        public override ISettings GetDefault()
        {
            return new UserHelpTourSettings
                       {
                           ModuleHelpTour = new Dictionary<Guid, int>(),
                           IsNewUser = false
                       };
        }
    }
        
    public class UserHelpTourHelper
    {
        public static bool IsNewUser
        {
            get { return UserHelpTourSettings.LoadForCurrentUser().IsNewUser; }
            set
            {
                var settings = UserHelpTourSettings.LoadForCurrentUser();
                settings.IsNewUser = value;
                settings.SaveForCurrentUser();
            }
        }
    }
}