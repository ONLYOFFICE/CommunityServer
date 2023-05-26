/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
        /// Returns the mail common settings.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.MailCommonSettings, ASC.Mail">Mail common settings</returns>
        /// <short>Get the mail common settings</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings")]
        public MailCommonSettings GetCommonSettings()
        {
            var commonSettings = MailCommonSettings.LoadForCurrentUser();
            return commonSettings;
        }

        /// <summary>
        /// Returns a flag that specifies whether to group messages into conversations or not.
        /// </summary>
        /// <returns>Boolean value: true - the flag is enabled, false - the flag is disabled</returns>
        /// <short>Get the "Enable Conversations" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/conversationsEnabled</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/conversationsEnabled")]
        public bool GetEnableConversationFlag()
        {
            var value = MailCommonSettings.EnableConversations;
            return value;
        }

        /// <summary>
        /// Sets a flag that specifies whether to group messages into conversations or not.
        /// </summary>
        /// <param type="System.Boolean, System" name="enabled">Specifies whether to group messages into conversations or not</param>
        /// <short>Set the "Enable Conversations" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/conversationsEnabled</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"settings/conversationsEnabled")]
        public void SetEnableConversationFlag(bool enabled)
        {
            MailCommonSettings.EnableConversations = enabled;
        }

        /// <summary>
        /// Returns a flag that specifies whether to display external images in the messages or not.
        /// </summary>
        /// <returns>Boolean value: true - the flag is enabled, false - the flag is disabled</returns>
        /// <short>Get the "Always display external images" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/alwaysDisplayImages</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/alwaysDisplayImages")]
        public bool GetAlwaysDisplayImagesFlag()
        {
            var value = MailCommonSettings.AlwaysDisplayImages;
            return value;
        }

        /// <summary>
        /// Sets a flag that specifies whether to display external images in the messages or not.
        /// </summary>
        /// <param type="System.Boolean, System" name="enabled">Specifies whether to display external images in the messages or not</param>
        /// <short>Set the "Always display external images" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/alwaysDisplayImages</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"settings/alwaysDisplayImages")]
        public void SetAlwaysDisplayImagesFlag(bool enabled)
        {
            MailCommonSettings.AlwaysDisplayImages = enabled;
        }

        /// <summary>
        /// Returns a flag that specifies whether to cache unread messages or not.
        /// </summary>
        /// <returns>Boolean value: true - the flag is enabled, false - the flag is disabled</returns>
        /// <short>Get the "Cache unread messages" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/cacheMessagesEnabled</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/cacheMessagesEnabled")]
        public bool GetCacheUnreadMessagesFlag()
        {
            //TODO: Change cache algorithm and restore it back.

            /*var value = MailCommonSettings.CacheUnreadMessages;
            return value;*/

            return false;
        }

        /// <summary>
        /// Sets a flag that specifies whether to cache unread messages or not.
        /// </summary>
        /// <param type="System.Boolean, System" name="enabled">Specifies whether to cache unread messages or not</param>
        /// <short>Set the "Cache unread messages" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/cacheMessagesEnabled</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"settings/cacheMessagesEnabled")]
        public void SetCacheUnreadMessagesFlag(bool enabled)
        {
            MailCommonSettings.CacheUnreadMessages = enabled;
        }

        /// <summary>
        /// Returns a flag that specifies whether to go to the next message after moving/deleting the currently viewed or return to the current folder.
        /// </summary>
        /// <returns>Boolean value: true - the flag is enabled, false - the flag is disabled</returns>
        /// <short>Get the "Go next after move" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/goNextAfterMoveEnabled</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/goNextAfterMoveEnabled")]
        public bool GetEnableGoNextAfterMoveFlag()
        {
            var value = MailCommonSettings.GoNextAfterMove;
            return value;
        }

        /// <summary>
        /// Sets a flag that specifies whether to go to the next message after moving/deleting the currently viewed or return to the current folder.
        /// </summary>
        /// <param type="System.Boolean, System" name="enabled">Specifies whether to go to the next message after moving/deleting the currently viewed or return to the current folder</param>
        /// <short>Set the "Go next after move" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/goNextAfterMoveEnabled</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"settings/goNextAfterMoveEnabled")]
        public void SetEnableGoNextAfterMoveFlag(bool enabled)
        {
            MailCommonSettings.GoNextAfterMove = enabled;
        }

        /// <summary>
        /// Returns a flag that specifies whether to completely replace text of the email when inserting a template or not.
        /// </summary>
        /// <returns>Boolean value: true - the flag is enabled, false - the flag is disabled</returns>
        /// <short>Get the "Replace message body" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/replaceMessageBody</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/replaceMessageBody")]
        public bool GetEnableReplaceMessageBodyFlag()
        {
            var value = MailCommonSettings.ReplaceMessageBody;
            return value;
        }

        /// <summary>
        /// Sets a flag that specifies whether to completely replace text of the email when inserting a template or not.
        /// </summary>
        /// <param type="System.Boolean, System" name="enabled">Specifies whether to completely replace text of the email when inserting a template or not</param>
        /// <short>Set the "Replace message body" flag</short> 
        /// <category>Settings</category>
        /// <path>api/2.0/mail/settings/replaceMessageBody</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"settings/replaceMessageBody")]
        public void SetEnableReplaceMessageBodyFlag(bool enabled)
        {
            MailCommonSettings.ReplaceMessageBody = enabled;
        }
    }
}
