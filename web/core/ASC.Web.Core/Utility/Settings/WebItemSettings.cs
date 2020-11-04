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

namespace ASC.Web.Core.Utility.Settings
{
    [Serializable]
    [DataContract]
    public class WebItemSettings : BaseSettings<WebItemSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{C888CF56-585B-4c78-9E64-FE1093649A62}"); }
        }

        [DataMember(Name = "Settings")]
        public List<WebItemOption> SettingsCollection { get; set; }


        public WebItemSettings()
        {
            SettingsCollection = new List<WebItemOption>();
        }


        public override ISettings GetDefault()
        {
            var settings = new WebItemSettings();
            WebItemManager.Instance.GetItemsAll().ForEach(w =>
            {
                var opt = new WebItemOption()
                {
                    ItemID = w.ID,
                    SortOrder = WebItemManager.GetSortOrder(w),
                    Disabled = false,
                };
                settings.SettingsCollection.Add(opt);
            });
            return settings;
        }


        [Serializable]
        [DataContract]
        public class WebItemOption
        {
            [DataMember(Name = "Id")]
            public Guid ItemID { get; set; }

            [DataMember(Name = "SortOrder")]
            public int SortOrder { get; set; }

            [DataMember(Name = "Disabled")]
            public bool Disabled { get; set; }
        }
    }
}