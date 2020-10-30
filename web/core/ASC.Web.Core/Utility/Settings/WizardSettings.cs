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

namespace ASC.Web.Core.Utility.Settings
{
    [Serializable]
    [DataContract]
    public class WizardSettings : BaseSettings<WizardSettings>
    {
        [DataMember(Name = "Analytics")]
        public bool Analytics { get; set; }

        [DataMember(Name = "Completed")]
        public bool Completed { get; set; }

        public override Guid ID
        {
            get { return new Guid("{9A925891-1F92-4ed7-B277-D6F649739F06}"); }
        }


        public override ISettings GetDefault()
        {
            return new WizardSettings
                {
                    Analytics = false,
                    Completed = true
                };
        }
    }
}
