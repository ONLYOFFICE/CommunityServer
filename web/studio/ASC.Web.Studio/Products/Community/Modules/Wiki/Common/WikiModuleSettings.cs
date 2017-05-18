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
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Web.Community.Wiki.Common
{
    [Serializable]
    [DataContract]
    public class WikiModuleSettings: ISettings
    {

        #region static functions
        public static WikiModuleSettings GetSettings(Guid userID)
        {
            return SettingsManager.Instance.LoadSettingsFor<WikiModuleSettings>(userID);
        }

        public static bool SetSettings(WikiModuleSettings settings, Guid userID)
        {
            return SettingsManager.Instance.SaveSettingsFor<WikiModuleSettings>(settings, userID);
        }


        public static bool GetIsWysiwygDefault(Guid userID)
        {
            return GetSettings(userID).IsWysiwygDefault;
        }

        public static void SetIsWysiwygDefault(bool isWysiwygDefault, Guid userID)
        {
            WikiModuleSettings currentSettings = GetSettings(userID);
            currentSettings.IsWysiwygDefault = isWysiwygDefault;
            SetSettings(currentSettings, userID);
        }

        #endregion

        [DataMember(Name = "IsWysiwygDefault")]
        public bool IsWysiwygDefault { get; set; }

        #region ISettings Members 
        public ISettings GetDefault()
        {
            return new WikiModuleSettings() { IsWysiwygDefault = true };
        }

        public Guid ID
        {
            get { return new Guid("{9174797C-6CE3-40b8-852A-BC9D5F3AE794}"); }
        }

        #endregion
    }
}
