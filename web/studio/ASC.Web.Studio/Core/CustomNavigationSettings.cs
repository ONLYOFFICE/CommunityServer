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
