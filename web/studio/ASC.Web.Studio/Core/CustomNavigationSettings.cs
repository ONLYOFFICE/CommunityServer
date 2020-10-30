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

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class CustomNavigationSettings : BaseSettings<CustomNavigationSettings>
    {       
        [DataMember]
        public List<CustomNavigationItem> Items { get; set; }

        public override Guid ID
        {
            get { return new Guid("{32E02E4C-925D-4391-BAA4-3B5D223A2104}"); }
        }

        public override ISettings GetDefault()
        {
            return new CustomNavigationSettings { Items = new List<CustomNavigationItem>() };
        }
    }

    [Serializable]
    [DataContract]
    public class CustomNavigationItem 
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string BigImg { get; set; }

        [DataMember]
        public string SmallImg { get; set; }

        [DataMember]
        public bool ShowInMenu { get; set; }

        [DataMember]
        public bool ShowOnHomePage { get; set; }

        private static string GetDefaultBigImg()
        {
            return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkAgMAAAANjH3HAAAADFBMVEUAAADJycnJycnJycmiuNtHAAAAA3RSTlMAf4C/aSLHAAAAyElEQVR4Xu3NsQ3CMBSE4YubFB4ilHQegdGSjWACvEpGoEyBYiL05AdnXUGHolx10lf82MmOpfLeo5UoJUhBlpKkRCnhUy7b9XCWkqQMUkYlXVHSf8kTvkHKqKQrSnopg5SRxTMklLmS1MwaSWpmCSQ1MyOzWGZCYrEMEFksA4QqlAFuJJYBcCKxjM3FMySeIfEMC2dMOONCGZZgmdr1ly3TSrJMK9EyJBaaGrHQikYstAiJZRYSyiQEdyg5S8Evckih/YPscsdej0H6dc0TYw4AAAAASUVORK5CYII=";
        }

        private static string GetDefaultSmallImg()
        {
            return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAUUlEQVR4AWMY/KC5o/cAEP9HxxgKcSpCGELYADyu2E6mAQjNxBlAWPNxkHdwGkBIM3KYYDUAr2ZCAE+oH8eujrAXDsA0k2EAAtDXAGLx4MpsADUgvkRKUlqfAAAAAElFTkSuQmCC";
        }

        public static CustomNavigationItem GetSample()
        {
            return new CustomNavigationItem
            {
                Id = Guid.Empty,
                ShowInMenu = true,
                ShowOnHomePage = true,
                BigImg = GetDefaultBigImg(),
                SmallImg = GetDefaultSmallImg()
            };
        }
    }
}
