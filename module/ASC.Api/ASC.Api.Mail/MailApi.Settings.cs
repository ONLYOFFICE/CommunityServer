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


using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

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
            //TODO: Change cache algoritnm and restore it back

            /*var value = MailCommonSettings.CacheUnreadMessages;
            return value;*/

            return false;
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

        /// <summary>
        ///    Returns ReplaceMessageBody flag
        /// </summary>
        /// <returns>boolean</returns>
        /// <short>Get ReplaceMessageBody flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/replaceMessageBody")]
        public bool GetEnableReplaceMessageBodyFlag()
        {
            var value = MailCommonSettings.ReplaceMessageBody;
            return value;
        }

        /// <summary>
        ///    Set ReplaceMessageBody flag
        /// </summary>
        /// <param name="enabled">True or False value</param>
        /// <short>Set ReplaceMessageBody flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/replaceMessageBody")]
        public void SetEnableReplaceMessageBodyFlag(bool enabled)
        {
            MailCommonSettings.ReplaceMessageBody = enabled;
        }
    }
}
