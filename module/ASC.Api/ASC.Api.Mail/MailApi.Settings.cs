/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
        /// Returns the common settings.
        /// </summary>
        /// <returns>Common settings</returns>
        /// <short>Get common settings</short> 
        /// <category>Settings</category>
        [Read(@"settings")]
        public MailCommonSettings GetCommonSettings()
        {
            var commonSettings = MailCommonSettings.LoadForCurrentUser();
            return commonSettings;
        }

        /// <summary>
        /// Returns the "Enable conversations" flag.
        /// </summary>
        /// <returns>Boolean value: True - the flag is enabled, False - the flag is disabled</returns>
        /// <short>Get the "Enable conversations" flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/conversationsEnabled")]
        public bool GetEnableConversationFlag()
        {
            var value = MailCommonSettings.EnableConversations;
            return value;
        }

        /// <summary>
        /// Sets the "Enable Conversations" flag.
        /// </summary>
        /// <param name="enabled">Specifies if the "Enable Conversations" flag is enabled or not</param>
        /// <short>Set the "Enable Conversations" flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/conversationsEnabled")]
        public void SetEnableConversationFlag(bool enabled)
        {
            MailCommonSettings.EnableConversations = enabled;
        }

        /// <summary>
        /// Returns the "Always display images" flag.
        /// </summary>
        /// <returns>Boolean value: True - the flag is enabled, False - the flag is disabled</returns>
        /// <short>Get the "Always display images" flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/alwaysDisplayImages")]
        public bool GetAlwaysDisplayImagesFlag()
        {
            var value = MailCommonSettings.AlwaysDisplayImages;
            return value;
        }

        /// <summary>
        /// Sets the "Always display images" flag.
        /// </summary>
        /// <param name="enabled">Specifies if the "Always display images" flag is enabled or not</param>
        /// <short>Set the "Always display images" flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/alwaysDisplayImages")]
        public void SetAlwaysDisplayImagesFlag(bool enabled)
        {
            MailCommonSettings.AlwaysDisplayImages = enabled;
        }

        /// <summary>
        /// Returns the "Cache unread messages" flag.
        /// </summary>
        /// <returns>Boolean value: True - the flag is enabled, False - the flag is disabled</returns>
        /// <short>Get the "Cache unread messages" flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/cacheMessagesEnabled")]
        public bool GetCacheUnreadMessagesFlag()
        {
            //TODO: Change cache algorithm and restore it back.

            /*var value = MailCommonSettings.CacheUnreadMessages;
            return value;*/

            return false;
        }

        /// <summary>
        /// Sets the "Cache unread messages" flag.
        /// </summary>
        /// <param name="enabled">Specifies if the "Cache unread messages" flag is enabled or not</param>
        /// <short>Set the "Cache unread messages" flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/cacheMessagesEnabled")]
        public void SetCacheUnreadMessagesFlag(bool enabled)
        {
            MailCommonSettings.CacheUnreadMessages = enabled;
        }

        /// <summary>
        /// Returns the "Go next after move" flag.
        /// </summary>
        /// <returns>Boolean value: True - the flag is enabled, False - the flag is disabled</returns>
        /// <short>Get the "Go next after move" flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/goNextAfterMoveEnabled")]
        public bool GetEnableGoNextAfterMoveFlag()
        {
            var value = MailCommonSettings.GoNextAfterMove;
            return value;
        }

        /// <summary>
        /// Sets the "Go next after move" flag.
        /// </summary>
        /// <param name="enabled">Specifies if the "Go next after move" flag is enabled or not</param>
        /// <short>Set the "Go next after move" flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/goNextAfterMoveEnabled")]
        public void SetEnableGoNextAfterMoveFlag(bool enabled)
        {
            MailCommonSettings.GoNextAfterMove = enabled;
        }

        /// <summary>
        /// Returns the "Replace message body" flag.
        /// </summary>
        /// <returns>Boolean value: True - the flag is enabled, False - the flag is disabled</returns>
        /// <short>Get the "Replace message body" flag</short> 
        /// <category>Settings</category>
        [Read(@"settings/replaceMessageBody")]
        public bool GetEnableReplaceMessageBodyFlag()
        {
            var value = MailCommonSettings.ReplaceMessageBody;
            return value;
        }

        /// <summary>
        /// Sets the "Replace message body" flag.
        /// </summary>
        /// <param name="enabled">Specifies if the "Replace message body" flag is enabled or not</param>
        /// <short>Set the "Replace message body" flag</short> 
        /// <category>Settings</category>
        [Update(@"settings/replaceMessageBody")]
        public void SetEnableReplaceMessageBodyFlag(bool enabled)
        {
            MailCommonSettings.ReplaceMessageBody = enabled;
        }
    }
}
