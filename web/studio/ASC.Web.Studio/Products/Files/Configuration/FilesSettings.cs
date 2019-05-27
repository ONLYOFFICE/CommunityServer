/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;
using ASC.Files.Core;

namespace ASC.Web.Files.Classes
{
    [Serializable]
    [DataContract]
    public class FilesSettings : BaseSettings<FilesSettings>
    {
        [DataMember(Name = "EnableThirdpartySettings")]
        public bool EnableThirdpartySetting { get; set; }

        [DataMember(Name = "FastDelete")]
        public bool FastDeleteSetting { get; set; }

        [DataMember(Name = "StoreOriginalFiles")]
        public bool StoreOriginalFilesSetting { get; set; }

        [DataMember(Name = "UpdateIfExist")]
        public bool UpdateIfExistSetting { get; set; }

        [DataMember(Name = "ConvertNotify")]
        public bool ConvertNotifySetting { get; set; }

        [DataMember(Name = "SortedBy")]
        public SortedByType DefaultSortedBySetting { get; set; }

        [DataMember(Name = "SortedAsc")]
        public bool DefaultSortedAscSetting { get; set; }

        public override ISettings GetDefault()
        {
            return new FilesSettings
                {
                    FastDeleteSetting = false,
                    EnableThirdpartySetting = true,
                    StoreOriginalFilesSetting = true,
                    UpdateIfExistSetting = false,
                    ConvertNotifySetting = true,
                    DefaultSortedBySetting = SortedByType.DateAndTime,
                    DefaultSortedAscSetting = false
                };
        }

        public override Guid ID
        {
            get { return new Guid("{03B382BD-3C20-4f03-8AB9-5A33F016316E}"); }
        }

        public static bool ConfirmDelete
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.FastDeleteSetting = !value;
                setting.SaveForCurrentUser();
            }
            get { return !LoadForCurrentUser().FastDeleteSetting; }
        }

        public static bool EnableThirdParty
        {
            set { new FilesSettings { EnableThirdpartySetting = value }.Save(); }
            get { return Load().EnableThirdpartySetting; }
        }

        public static bool StoreOriginalFiles
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.StoreOriginalFilesSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().StoreOriginalFilesSetting; }
        }

        public static bool UpdateIfExist
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.UpdateIfExistSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().UpdateIfExistSetting; }
        }

        public static bool ConvertNotify
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.ConvertNotifySetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().ConvertNotifySetting; }
        }

        public static OrderBy DefaultOrder
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.DefaultSortedBySetting = value.SortedBy;
                setting.DefaultSortedAscSetting = value.IsAsc;
                setting.SaveForCurrentUser();
            }
            get
            {
                var setting = LoadForCurrentUser();
                return new OrderBy(setting.DefaultSortedBySetting, setting.DefaultSortedAscSetting);
            }
        }
    }
}