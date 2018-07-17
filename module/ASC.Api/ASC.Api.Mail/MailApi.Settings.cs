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


using ASC.Api.Attributes;
using ASC.Mail.Aggregator.Common;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns Common Settings
        /// </summary>
        /// <returns>MailCommonSettings object</returns>
        /// <short>Get common settings</short> 
        /// <category>Settings</category>
        [Read(@"settings")]
        public MailCommonSettings GetCommonSettings()
        {
            var commonSettings = MailCommonSettings.LoadForCurrentUser();
            return commonSettings;
        }

        /// <summary>
        ///    Returns EnableConversations flag
        /// </summary>
        /// <returns>boolean</returns>
        /// <short>Get EnableConversations flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/conversationsEnabled")]
        public bool GetEnableConversationFlag()
        {
            var value = MailCommonSettings.EnableConversations;
            return value;
        }

        /// <summary>
        ///    Set EnableConversations flag
        /// </summary>
        /// <param name="enabled">True or False value</param>
        /// <short>Set EnableConversations flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/conversationsEnabled")]
        public void SetEnableConversationFlag(bool enabled)
        {
            MailCommonSettings.EnableConversations = enabled;
        }

        /// <summary>
        ///    Returns AlwaysDisplayImages flag
        /// </summary>
        /// <returns>boolean</returns>
        /// <short>Get AlwaysDisplayImages flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/alwaysDisplayImages")]
        public bool GetAlwaysDisplayImagesFlag()
        {
            var value = MailCommonSettings.AlwaysDisplayImages;
            return value;
        }

        /// <summary>
        ///    Set AlwaysDisplayImages flag
        /// </summary>
        /// <param name="enabled">True or False value</param>
        /// <short>Set AlwaysDisplayImages flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/alwaysDisplayImages")]
        public void SetAlwaysDisplayImagesFlag(bool enabled)
        {
            MailCommonSettings.AlwaysDisplayImages = enabled;
        }

        /// <summary>
        ///    Returns CacheUnreadMessages flag
        /// </summary>
        /// <returns>boolean</returns>
        /// <short>Get CacheUnreadMessages flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/cacheMessagesEnabled")]
        public bool GetCacheUnreadMessagesFlag()
        {
            var value = MailCommonSettings.CacheUnreadMessages;
            return value;
        }

        /// <summary>
        ///    Set CacheUnreadMessages flag
        /// </summary>
        /// <param name="enabled">True or False value</param>
        /// <short>Set CacheUnreadMessages flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/cacheMessagesEnabled")]
        public void SetCacheUnreadMessagesFlag(bool enabled)
        {
            MailCommonSettings.CacheUnreadMessages = enabled;
        }

        /// <summary>
        ///    Returns GoNextAfterMove flag
        /// </summary>
        /// <returns>boolean</returns>
        /// <short>Get GoNextAfterMove flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/goNextAfterMoveEnabled")]
        public bool GetEnableGoNextAfterMoveFlag()
        {
            var value = MailCommonSettings.GoNextAfterMove;
            return value;
        }

        /// <summary>
        ///    Set GoNextAfterMove flag
        /// </summary>
        /// <param name="enabled">True or False value</param>
        /// <short>Set GoNextAfterMove flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/goNextAfterMoveEnabled")]
        public void SetEnableGoNextAfterMoveFlag(bool enabled)
        {
            MailCommonSettings.GoNextAfterMove = enabled;
        }
    }
}
