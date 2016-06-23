/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core.Utility.Settings
{
    [Serializable]
    [DataContract]
    public class WebItemSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("{C888CF56-585B-4c78-9E64-FE1093649A62}"); }
        }

        [DataMember(Name = "Settings")]
        public List<WebItemOption> SettingsCollection { get; set; }


        public WebItemSettings()
        {
            SettingsCollection = new List<WebItemOption>();
        }

        
        public ISettings GetDefault()
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
