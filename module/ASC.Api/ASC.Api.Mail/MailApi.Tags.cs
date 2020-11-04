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
using System.Linq;
using System.Threading;
using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;
using ASC.Web.Mail.Resources;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns list of the tags used in Mail
        /// </summary>
        /// <returns>Tags list. Tags represented as JSON.</returns>
        /// <short>Get tags list</short> 
        /// <category>Tags</category>
        [Read(@"tags")]
        public IEnumerable<MailTagData> GetTags()
        {
            return MailEngineFactory.TagEngine.GetTags().ToTagData();
        }

        /// <summary>
        ///    Creates a new tag
        /// </summary>
        /// <param name="name">Tag name represented as string</param>
        /// <param name="style">Style identificator. With postfix will be added to tag css style whe it will represent. Specifies color of tag.</param>
        /// <param name="addresses">Specifies list of addresses tag associated with.</param>
        /// <returns>MailTag</returns>
        /// <short>Create tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Create(@"tags")]
        public MailTagData CreateTag(string name, string style, IEnumerable<string> addresses)
        {
            //TODO: Is it necessary?
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            if (MailEngineFactory.TagEngine.IsTagExists(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1", "\"" + name + "\""));

            return MailEngineFactory.TagEngine.CreateTag(name, style, addresses).ToTagData();

        }

        /// <summary>
        ///    Updates the selected tag
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name">Tag name represented as string</param>
        /// <param name="style">Style identificator. With postfix will be added to tag css style whe it will represent. Specifies color of tag.</param>
        /// <param name="addresses">Specifies list of addresses tag associated with.</param>
        /// <returns>Updated MailTag</returns>
        /// <short>Update tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}")]
        public MailTagData UpdateTag(int id, string name, string style, IEnumerable<string> addresses)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid tag id", "id");

            //TODO: Is it necessary?
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            try
            {
                var tag = MailEngineFactory.TagEngine.UpdateTag(id, name, style, addresses);

                return tag.ToTagData();
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Equals("Tag name already exists"))
                    throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1",
                        "\"" + name + "\""));

                throw;
            }
        }

        /// <summary>
        ///    Deletes the selected tag from TLMail
        /// </summary>
        /// <param name="id">Tag for deleting id</param>
        /// <returns>Deleted MailTag</returns>
        /// <short>Delete tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Delete(@"tags/{id}")]
        public int DeleteTag(int id)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid tag id", "id");

            if (!MailEngineFactory.TagEngine.DeleteTag(id))
                throw new Exception("DeleteTag failed");

            return id;
        }

        /// <summary>
        ///    Adds the selected tag to the messages
        /// </summary>
        /// <param name="id">Tag for setting id</param>
        /// <param name="messages">Messages id for setting.</param>
        /// <returns>Setted MailTag</returns>
        /// <short>Set tag to messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/set")]
        public int SetTag(int id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Messages are empty", "messages");

            MailEngineFactory.TagEngine.SetMessagesTag(messages, id);

            return id;
        }

        /// <summary>
        ///    Removes the specified tag from messages
        /// </summary>
        /// <param name="id">Tag for removing id</param>
        /// <param name="messages">Messages id for removing.</param>
        /// <returns>Removed mail tag</returns>
        /// <short>Remove tag from messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when parameters are invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/unset")]
        public int UnsetTag(int id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Messages are empty", "messages");

            MailEngineFactory.TagEngine.UnsetMessagesTag(messages, id);

            return id;
        }
    }
}
